package com.microsoft.cognitiveservices.speech.audio;
//
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE.md file in the project root for full license information.
//

import com.microsoft.cognitiveservices.speech.util.Contracts;
import com.microsoft.cognitiveservices.speech.SpeechConfig;

/**
 * Represents audio input configuration used for specifying what type of input to use (microphone, file, stream).
 */
public final class AudioConfig
{
    // load the native library.
    static {
        // trigger loading of native library
        try {
            Class.forName(SpeechConfig.class.getName());
        }
        catch (ClassNotFoundException ex) {
            throw new IllegalStateException(ex);
        }
    }

    /**
     * Creates an AudioConfig object representing the default microphone on the system.
     * @return The audio input configuration being created.
     */
    public static com.microsoft.cognitiveservices.speech.audio.AudioConfig fromDefaultMicrophoneInput() {
        return new AudioConfig(com.microsoft.cognitiveservices.speech.internal.AudioConfig.FromDefaultMicrophoneInput());
    }

    /**
     * Creates an AudioConfig object representing the specified file.
     * @param fileName Specifies the audio input file. Currently, only WAV / PCM with 16-bit samples, 16 kHz sample rate, and a single channel (Mono) is supported.
     * @return The audio input configuration being created.
     */
    public static com.microsoft.cognitiveservices.speech.audio.AudioConfig fromWavFileInput(String fileName) {
        return new AudioConfig(com.microsoft.cognitiveservices.speech.internal.AudioConfig.FromWavFileInput(fileName));
    }

    /**
     * Creates an AudioConfig object representing the specified stream.
     * @param audioStream Specifies the custom audio input stream. Currently, only WAV / PCM with 16-bit samples, 16 kHz sample rate, and a single channel (Mono) is supported.
     * @return The audio input configuration being created.
     */
    public static com.microsoft.cognitiveservices.speech.audio.AudioConfig fromStreamInput(AudioInputStream audioStream) {
        return new AudioConfig(com.microsoft.cognitiveservices.speech.internal.AudioConfig.FromStreamInput(audioStream.getStreamImpl()), audioStream);
    }

    /**
     * Creates an AudioConfig object representing the specified stream.
     * @param callback Specifies the pull audio input stream callback. Currently, only WAV / PCM with 16-bit samples, 16 kHz sample rate, and a single channel (Mono) is supported.
     * @return The audio input configuration being created.
     */
    public static com.microsoft.cognitiveservices.speech.audio.AudioConfig fromStreamInput(PullAudioInputStreamCallback callback) {
        PullAudioInputStream pullStream = PullAudioInputStream.create(callback);
        return new AudioConfig(com.microsoft.cognitiveservices.speech.internal.AudioConfig.FromStreamInput(pullStream.getStreamImpl()), pullStream);
    }

    /**
     * Explicitly frees any external resource attached to the object
     */
    public void close() {
        if (this._configImpl != null) {
            this._configImpl.delete();
        }
        this._configImpl = null;
    }

    AudioConfig(com.microsoft.cognitiveservices.speech.internal.AudioConfig config) {
        Contracts.throwIfNull(config, "config");
        this._configImpl = config;
        this._streamKeepAlive = null;
    }

    AudioConfig(com.microsoft.cognitiveservices.speech.internal.AudioConfig config, com.microsoft.cognitiveservices.speech.audio.AudioInputStream audioStream) {
        Contracts.throwIfNull(config, "config");
        this._configImpl = config;
        this._streamKeepAlive = audioStream;
    }

    private com.microsoft.cognitiveservices.speech.internal.AudioConfig _configImpl;
    @SuppressWarnings("unused")
    private AudioInputStream _streamKeepAlive;

    /*! \cond INTERNAL */

    /**
     * Returns the audio input configuration.
     * @return The implementation of the config.
     */
    public com.microsoft.cognitiveservices.speech.internal.AudioConfig getConfigImpl() {
        return this._configImpl;
    }

    /*! \endcond */
}
