//
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE.md file in the project root for full license information.
//

using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.CognitiveServices.Speech
{
    /// <summary>
    /// Performs speech recognition from microphone, file, or other audio input streams, and gets transcribed text as result.
    /// </summary>
    /// <example>
    /// An example to use the speech recognizer from microphone and listen to events generated by the recognizer.
    /// <code>
    /// public async Task SpeechContinuousRecognitionAsync()
    /// {
    ///     // Creates an instance of a speech factory with specified subscription key and service region.
    ///     // Replace with your own subscription key and service region (e.g., "westus").
    ///     var factory = SpeechFactory.FromSubscription("YourSubscriptionKey", "YourServiceRegion");
    ///
    ///     // Creates a speech recognizer from microphone.
    ///     using (var recognizer = factory.CreateSpeechRecognizer())
    ///     {
    ///         // Subscribes to events.
    ///         recognizer.IntermediateResultReceived += (s, e) => {
    ///             Console.WriteLine($"\n    Partial result: {e.Result.Text}.");
    ///         };
    ///
    ///         recognizer.FinalResultReceived += (s, e) => {
    ///             var result = e.Result;
    ///             Console.WriteLine($"Recognition status: {result.RecognitionStatus.ToString()}");
    ///             if (result.RecognitionStatus == RecognitionStatus.Recognized)
    ///             {
    ///                     Console.WriteLine($"Final result: Text: {result.Text}."); 
    ///             }
    ///         };
    ///
    ///         recognizer.RecognitionErrorRaised += (s, e) => {
    ///             Console.WriteLine($"\n    An error occurred. Status: {e.Status.ToString()}, FailureReason: {e.FailureReason}");
    ///         };
    ///
    ///         recognizer.OnSessionEvent += (s, e) => {
    ///             Console.WriteLine($"\n    Session event. Event: {e.EventType.ToString()}.");
    ///         };
    ///
    ///         // Starts continuous recognition. Uses StopContinuousRecognitionAsync() to stop recognition.
    ///         await recognizer.StartContinuousRecognitionAsync().ConfigureAwait(false);
    ///
    ///         do
    ///         {
    ///             Console.WriteLine("Press Enter to stop");
    ///         } while (Console.ReadKey().Key != ConsoleKey.Enter);
    ///
    ///         // Stops recognition.
    ///         await recognizer.StopContinuousRecognitionAsync().ConfigureAwait(false);
    ///     }
    /// }
    /// </code>
    /// </example>
    public sealed class SpeechRecognizer : Recognizer
    {
        /// <summary>
        /// The event <see cref="IntermediateResultReceived"/> signals that an intermediate recognition result is received.
        /// </summary>
        public event EventHandler<SpeechRecognitionResultEventArgs> IntermediateResultReceived;

        /// <summary>
        /// The event <see cref="FinalResultReceived"/> signals that a final recognition result is received.
        /// </summary>
        public event EventHandler<SpeechRecognitionResultEventArgs> FinalResultReceived;

        /// <summary>
        /// The event <see cref="RecognitionErrorRaised"/> signals that an error occurred during recognition.
        /// </summary>
        public event EventHandler<RecognitionErrorEventArgs> RecognitionErrorRaised;

        internal SpeechRecognizer(Internal.SpeechRecognizer recoImpl)
        {
            this.recoImpl = recoImpl;

            intermediateResultHandler = new ResultHandlerImpl(this, isFinalResultHandler: false);
            recoImpl.IntermediateResult.Connect(intermediateResultHandler);

            finalResultHandler = new ResultHandlerImpl(this, isFinalResultHandler: true);
            recoImpl.FinalResult.Connect(finalResultHandler);

            errorHandler = new ErrorHandlerImpl(this);
            recoImpl.Canceled.Connect(errorHandler);

            recoImpl.SessionStarted.Connect(sessionStartedHandler);
            recoImpl.SessionStopped.Connect(sessionStoppedHandler);
            recoImpl.SpeechStartDetected.Connect(speechStartDetectedHandler);
            recoImpl.SpeechEndDetected.Connect(speechEndDetectedHandler);

            Parameters = new RecognizerParametersImpl(recoImpl.Parameters);
        }

        internal SpeechRecognizer(Internal.SpeechRecognizer recoImpl, AudioInputStream inputStream) : this(recoImpl)
        {
            streamInput = inputStream;
        }

        /// <summary>
        /// Gets/sets the deployment id of a customized speech model that is used for speech recognition.
        /// </summary>
        public string DeploymentId
        {
            get
            {
                return Parameters.Get<string>(SpeechParameterNames.DeploymentId);
            }

            set
            {
                recoImpl.SetDeploymentId(value);
            }
        }

        /// <summary>
        /// Gets the language name that was set when the recognizer was created.
        /// </summary>
        public string Language
        {
            get
            {
                return Parameters.Get<string>(SpeechParameterNames.RecognitionLanguage);
            }
        }

        /// <summary>
        /// Gets the output format setting.
        /// </summary>
        public OutputFormat OutputFormat
        {
            get
            {
                return Parameters.Get<string>(SpeechParameterNames.OutputFormat) ==  OutputFormatParameterValues.Detailed ? OutputFormat.Detailed : OutputFormat.Simple;
            }
        }

        /// <summary>
        /// The collection of parameters and their values defined for this <see cref="SpeechRecognizer"/>.
        /// </summary>
        public IRecognizerParameters Parameters { get; internal set; }

        /// <summary>
        /// Starts speech recognition, and stops after the first utterance is recognized. The task returns the recognition text as result.
        /// Note: RecognizeAsync() returns when the first utterance has been recognized, so it is suitable only for single shot recognition like command or query. For long-running recognition, use StartContinuousRecognitionAsync() instead.
        /// </summary>
        /// <returns>A task representing the recognition operation. The task returns a value of <see cref="SpeechRecognitionResult"/> </returns>
        /// <example>
        /// The following example creates a speech recognizer, and then gets and prints the recognition result.
        /// <code>
        /// public async Task SpeechSingleShotRecognitionAsync()
        /// {
        ///     // Creates an instance of a speech factory with specified subscription key and service region.
        ///     // Replace with your own subscription key and service region (e.g., "westus").
        ///     var factory = SpeechFactory.FromSubscription("YourSubscriptionKey", "YourServiceRegion");
        ///
        ///     // Creates a speech recognizer using microphone as audio input. The default language is "en-us".
        ///     using (var recognizer = factory.CreateSpeechRecognizer())
        ///     {
        ///         // Starts recognizing.
        ///         Console.WriteLine("Say something...");
        ///
        ///         // Performs recognition.
        ///         // RecognizeAsync() returns when the first utterance has been recognized, so it is suitable 
        ///         // only for single shot recognition like command or query. For long-running recognition, use
        ///         // StartContinuousRecognitionAsync() instead.
        ///         var result = await recognizer.RecognizeAsync().ConfigureAwait(false);
        ///
        ///         // Checks result.
        ///         if (result.RecognitionStatus != RecognitionStatus.Recognized)
        ///         {
        ///             Console.WriteLine($"Recognition status: {result.RecognitionStatus.ToString()}");
        ///             if (result.RecognitionStatus == RecognitionStatus.Canceled)
        ///             {
        ///                 Console.WriteLine($"There was an error, reason: {result.RecognitionFailureReason}");
        ///             }
        ///             else
        ///             {
        ///                 Console.WriteLine("No speech could be recognized.\n");
        ///             }
        ///         }
        ///         else
        ///         {
        ///             Console.WriteLine($"We recognized: {result.Text}");
        ///         }
        ///     }
        /// }
        /// </code>
        /// </example>
        public Task<SpeechRecognitionResult> RecognizeAsync()
        {
            return Task.Run(() => { return new SpeechRecognitionResult(this.recoImpl.Recognize()); });
        }

        /// <summary>
        /// Starts speech recognition on a continuous audio stream, until StopContinuousRecognitionAsync() is called.
        /// User must subscribe to events to receive recognition results.
        /// </summary>
        /// <returns>A task representing the asynchronous operation that starts the recognition.</returns>
        public Task StartContinuousRecognitionAsync()
        {
            return Task.Run(() => { this.recoImpl.StartContinuousRecognition(); });
        }

        /// <summary>
        /// Stops continuous speech recognition.
        /// </summary>
        /// <returns>A task representing the asynchronous operation that stops the recognition.</returns>
        public Task StopContinuousRecognitionAsync()
        {
            return Task.Run(() => { this.recoImpl.StopContinuousRecognition(); });
        }

        /// <summary>
        /// Starts speech recognition on a continuous audio stream with keyword spotting, until StopKeywordRecognitionAsync() is called.
        /// User must subscribe to events to receive recognition results.
        /// Note: Key word spotting functionality is only available on the Cognitive Services Device SDK. This functionality is currently not included in the SDK itself.
        /// </summary>
        /// <param name="model">The keyword recognition model that specifies the keyword to be recognized.</param>
        /// <returns>A task representing the asynchronous operation that starts the recognition.</returns>
        public Task StartKeywordRecognitionAsync(KeywordRecognitionModel model)
        {
            return Task.Run(() => { this.recoImpl.StartKeywordRecognition(model.modelImpl); });
        }

        /// <summary>
        /// Stops continuous speech recognition with keyword spotting.
        /// Note: Key word spotting functionality is only available on the Cognitive Services Device SDK. This functionality is currently not included in the SDK itself.
        /// </summary>
        /// <returns>A task representing the asynchronous operation that stops the recognition.</returns>
        public Task StopKeywordRecognitionAsync()
        {
            return Task.Run(() => { this.recoImpl.StopKeywordRecognition(); });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposed)
            {
                return;
            }

            if (disposing)
            {
                recoImpl.IntermediateResult.Disconnect(intermediateResultHandler);
                recoImpl.FinalResult.Disconnect(finalResultHandler);
                recoImpl.Canceled.Disconnect(errorHandler);
                recoImpl.SessionStarted.Disconnect(sessionStartedHandler);
                recoImpl.SessionStopped.Disconnect(sessionStoppedHandler);
                recoImpl.SpeechStartDetected.Disconnect(speechStartDetectedHandler);
                recoImpl.SpeechEndDetected.Disconnect(speechEndDetectedHandler);

                intermediateResultHandler?.Dispose();
                finalResultHandler?.Dispose();
                errorHandler?.Dispose();
                recoImpl?.Dispose();
                disposed = true;
                base.Dispose(disposing);
            }
        }

        internal readonly Internal.SpeechRecognizer recoImpl;
        private readonly ResultHandlerImpl intermediateResultHandler;
        private readonly ResultHandlerImpl finalResultHandler;
        private readonly ErrorHandlerImpl errorHandler;
        private bool disposed = false;
        private readonly AudioInputStream streamInput;

        // Defines an internal class to raise a C# event for intermediate/final result when a corresponding callback is invoked by the native layer.
        private class ResultHandlerImpl : Internal.SpeechRecognitionEventListener
        {
            public ResultHandlerImpl(SpeechRecognizer recognizer, bool isFinalResultHandler)
            {
                this.recognizer = recognizer;
                this.isFinalResultHandler = isFinalResultHandler;
            }

            public override void Execute(Internal.SpeechRecognitionEventArgs eventArgs)
            {
                if (recognizer.disposed)
                {
                    return;
                }

                var resultEventArg = new SpeechRecognitionResultEventArgs(eventArgs);
                var handler = isFinalResultHandler ? recognizer.FinalResultReceived : recognizer.IntermediateResultReceived;
                if (handler != null)
                {
                    handler(this.recognizer, resultEventArg);
                }
            }

            private SpeechRecognizer recognizer;
            private bool isFinalResultHandler;
        }

        // Defines an internal class to raise a C# event for error during recognition when a corresponding callback is invoked by the native layer.
        private class ErrorHandlerImpl : Internal.SpeechRecognitionEventListener
        {
            public ErrorHandlerImpl(SpeechRecognizer recognizer)
            {
                this.recognizer = recognizer;
            }

            public override void Execute(Microsoft.CognitiveServices.Speech.Internal.SpeechRecognitionEventArgs eventArgs)
            {
                if (recognizer.disposed)
                {
                    return;
                }

                var resultEventArg = new RecognitionErrorEventArgs(eventArgs.SessionId, eventArgs.GetResult().Reason, eventArgs.GetResult().ErrorDetails);
                var handler = this.recognizer.RecognitionErrorRaised;

                if (handler != null)
                {
                    handler(this.recognizer, resultEventArg);
                }
            }

            private SpeechRecognizer recognizer;
        }
    }

}