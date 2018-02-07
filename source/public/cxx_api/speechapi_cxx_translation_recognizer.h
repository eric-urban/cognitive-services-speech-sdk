//
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE.md file in the project root for full license information.
//
// speechapi_cxx_translation_recognizer.h: Public API declarations for translation recognizer in C++.
//

#pragma once
#include <exception>
#include <future>
#include <memory>
#include <string>
#include <unordered_map>
#include <speechapi_c.h>
#include <speechapi_cxx_common.h>
#include <speechapi_cxx_recognition_async_recognizer.h>
#include <speechapi_cxx_translation_eventargs.h>
#include <speechapi_cxx_translation_result.h>


namespace CARBON_NAMESPACE_ROOT {
namespace Recognition {
namespace Translation {

// Defines scopes for requesting language resources.
// A client uses the scope define which sets of languages it is interested in.
//    TRANSLATION_LANGUAGE_RESOURCE_SCOPE_SPEECH: retrieves the set of languages available to transcribe speech into text.
//    TRANSLATION_LANGUAGE_RESOURCE_SCOPE_TEXT: retrieves the set of languages available to translate transcribed text.
//    TRANSLATION_LANGUAGE_RESOURCE_SCOPE_TTS: retrieves the set of languages and voices available to synthesize translated text back into speech.
// A client can retrieve multiple sets simultaneously by setting one or more values via bit-or. 
typedef unsigned int LanguageResourceScope;
#define TRANSLATION_LANGUAGE_RESOURCE_SCOPE_SPEECH 0x01
#define TRANSLATION_LANGUAGE_RESOURCE_SCOPE_TEXT 0x02
#define TRANSLATION_LANGUAGE_RESOURCE_SCOPE_TTS 0x04

/*
* Defines the format of resource value for the scope TRANSLATION_LANGUAGE_RESOURCE_SCOPE_SPEECH.
*    name: Display name of the language.
*    language: Language tag of the associated written language.
*/
typedef struct _SpeechScopeResourceValue
{
    ::std::wstring name;
    ::std::wstring language;
} SpeechScopeResourceValue;

/*
* Defines the format of resource value for the scope TRANSLATION_LANGUAGE_RESOURCE_SCOPE_TEXT.
*     name: Display name of the language.
**    dir: Directionality which is "rtl" for right-to-left languages, or "ltr" for left-to-right languages.
*/
typedef struct _TextScopeResourceValue
{
    ::std::wstring name;
    ::std::wstring dir;
} TextScopeResourceValue;

/*
* Defines the format of resource value for the scope TRANSLATION_LANGUAGE_RESOURCE_SCOPE_TTS
*   displayName: Display name for the voice.
*   gender : Gender of the voice(male or female).
*   locale : Language tag of the voice with primary language subtag and region subtag.
*   language : Language tag of the associated written language.
*   languageName : Display name of the language.
*   regionName : Display name of the region for this language.
*/
typedef struct _SynthesisScopeResourceValue
{
    ::std::wstring displayName;
    ::std::wstring gender;
    ::std::wstring locale;
    ::std::wstring languageName;
    ::std::wstring regionName;
    ::std::wstring language;
} SynthesisScopeResourceValue;

/*
* Defines language resources that are supported by the translation service.
* See https://docs.microsofttranslator.com/speech-translate.html for details.
*/
typedef struct _TranslationLanguageResource
{
    // Represents language resources associated with the speech-to-text property. It is a dictionary of (key, value) pairs.
    // Each key identifies a language supported for speech-to- text. The key is the identifier that client passes to the API.
    // The value associated with the key is of type SpeechScopeResourceValue.
    ::std::unordered_map<::std::wstring, SpeechScopeResourceValue> speechResources;

    // Represents language resources associated with the text property. It is also a dictionary where each key identifies a language
    // supported for text translation. The value associated with the key is of type TextScopeResourceValue.
    ::std::unordered_map<::std::wstring, TextScopeResourceValue> textResources;
    
    // Represents language resources associated with the text-to-speech property. It is a dictionary where each key identifies a 
    // supported voice. The value associated with the key is of type SynthesisScopeResourceValue.
    ::std::unordered_map<::std::wstring, SynthesisScopeResourceValue> voiceResources;

} TranslationLanguageResource;

/*
* The translation recognizer.
*/
class TranslationRecognizer final : virtual public AsyncRecognizer<TranslationResult, TranslationEventArgs>
{
public:
    
    /*
    * Gets available language resources supported by the translation service.
    * @param scopes: specifies which language scopes to query. Combing each scope (bitwise OR) is supported. See https://docs.microsofttranslator.com/speech-translate.html for details.
    * @param acceptLanguage: specifies the language (BCP 47 language tag), in which names of languages and regions are returned.
    * @return Supported translation language resources.
    */
    // TODO: Move this function outside the class as a global plain function in the namespace, after we have real implementation
    // in C API, and then exposed as C++ API. 
    // Keeping it here for now. Otherwise we have to create a separate CPP file to define this function, in order to pass compiler.  
    static TranslationLanguageResource GetLanguageResource(LanguageResourceScope scopes, ::std::wstring acceptLanguage)
    {
        TranslationLanguageResource discoveredResources{ {},{},{} };
        UNUSED(scopes);
        UNUSED(acceptLanguage);
        SPX_THROW_ON_FAIL(SPXERR_NOT_IMPL);
        return discoveredResources;
    }
    
    /*
    * Constructs a translation recognizer.
    * TODO: Other configuration options should be added either as constructor parameters or as separate options. Needs to be aligned with
    * that in Carbon speech recognition API.
    * @param sourceLanguage: Specifies the language of the incoming speech. The value must be one of the keys of TranslationLanguageResource.speechResource. 
    * @param targetlanguage: Specifies the language to translate the transcribed text into. The value must be one of the keys of TranslationLanguageResource.textResource.
    * @param requireVoiceOutput: The translation result includes translated audio of the final translation text.
    */
    TranslationRecognizer(const std::wstring& sourceLanguage, const std::wstring& targetLanguage, const bool requireVoiceOutput = false) :
        AsyncRecognizer(m_speechRecoParameters),
        OnTranslationIntermediateTextResult(m_onTranslationIntermediateTextResult),
        OnTranslationFinalTextResult(m_onTranslationFinalTextResult),
        OnTranslationAudioResult(m_onTranslationAudioResult),
        OnTranslationFullResult(m_onTranslationFullResult),
        OnTranslationError(m_onTranslationError)
    {
        UNUSED(sourceLanguage);
        UNUSED(targetLanguage);
        UNUSED(requireVoiceOutput);
        SPX_THROW_ON_FAIL(SPXERR_NOT_IMPL);
    };

    ~TranslationRecognizer() { };

    bool IsEnabled() override
    {
        SPX_THROW_ON_FAIL(SPXERR_NOT_IMPL);
        return false;
    };

    void Enable() override
    {
        SPX_THROW_ON_FAIL(SPXERR_NOT_IMPL);
    };

    void Disable() override
    {
        SPX_THROW_ON_FAIL(SPXERR_NOT_IMPL);
    };

    std::future<std::shared_ptr<TranslationResult>> RecognizeAsync() override
    {
        auto future = std::async(std::launch::async, [=]() -> std::shared_ptr<TranslationResult> {
            SPX_THROW_ON_FAIL(SPXERR_NOT_IMPL);

            return std::make_shared<TranslationResult>(nullptr);
        });

        return future;
    };

    std::future<void> StartContinuousRecognitionAsync() override
    {
        auto future = std::async(std::launch::async, [=]() -> void {
            SPX_THROW_ON_FAIL(SPXERR_NOT_IMPL);
        });

        return future;
    };

    std::future<void> StopContinuousRecognitionAsync() override
    {
        auto future = std::async(std::launch::async, [=]() -> void {
            SPX_THROW_ON_FAIL(SPXERR_NOT_IMPL);
        });

        return future;
    };

    /*
    * Defines translation specific events.
    * TODO: Currently, AsyncRecognizer defines events for speech recognitions. Some of events in AsyncRecognizer
    * is indeed not related to translation. The AsyncRecognizer should be refactored to only include common events,
    * and moves speech specific ones to SpeechRecognizer.
    */
    EventSignal<const TranslationEventArgs&>& OnTranslationIntermediateTextResult;
    EventSignal<const TranslationEventArgs&>& OnTranslationFinalTextResult;
    EventSignal<const TranslationEventArgs&>& OnTranslationAudioResult;
    EventSignal<const TranslationEventArgs&>& OnTranslationFullResult;
    EventSignal<const TranslationEventArgs&>& OnTranslationError;

private:

    TranslationRecognizer(const TranslationRecognizer&) = delete;
    TranslationRecognizer(const TranslationRecognizer&&) = delete;

    TranslationRecognizer& operator=(const TranslationRecognizer&) = delete;

    EventSignal<const TranslationEventArgs&> m_onTranslationIntermediateTextResult;
    EventSignal<const TranslationEventArgs&> m_onTranslationFinalTextResult;
    EventSignal<const TranslationEventArgs&> m_onTranslationAudioResult;
    EventSignal<const TranslationEventArgs&> m_onTranslationFullResult;
    EventSignal<const TranslationEventArgs&> m_onTranslationError;

    RecognizerParameters m_speechRecoParameters;
};


} } } // CARBON_NAMESPACE_ROOT::Recognition::Translation
