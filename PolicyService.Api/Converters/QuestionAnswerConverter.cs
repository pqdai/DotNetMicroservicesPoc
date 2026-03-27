using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PolicyService.Api.Commands.Dtos;

namespace PolicyService.Api.Converters;

internal class QuestionAnswerConverter : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return objectType.IsAssignableFrom(typeof(QuestionAnswer));
    }

    public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        var jsonObject = JObject.Load(reader);
        var target = Create(jsonObject);
        serializer.Populate(jsonObject.CreateReader(), target);
        return target;
    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        if (value is QuestionAnswer questionAnswer)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("questionCode");
            serializer.Serialize(writer, questionAnswer.QuestionCode);
            writer.WritePropertyName("questionType");
            serializer.Serialize(writer, questionAnswer.QuestionType);
            writer.WritePropertyName("answer");
            serializer.Serialize(writer, questionAnswer.GetAnswer());
            writer.WriteEndObject();
        }
    }

    private static QuestionAnswer Create(JObject jsonObject)
    {
        var questionTypeToken = jsonObject["questionType"]
            ?? throw new ApplicationException("questionType is required.");
        var questionCodeToken = jsonObject["questionCode"]
            ?? throw new ApplicationException("questionCode is required.");
        var answerToken = jsonObject["answer"]
            ?? throw new ApplicationException("answer is required.");

        var typeName = Enum.Parse<QuestionType>(questionTypeToken.ToString());
        switch (typeName)
        {
            case QuestionType.Text:
                return new TextQuestionAnswer
                {
                    QuestionCode = questionCodeToken.ToString(),
                    Answer = answerToken.ToString()
                };
            case QuestionType.Numeric:
                return new NumericQuestionAnswer
                {
                    QuestionCode = questionCodeToken.ToString(),
                    Answer = answerToken.Value<decimal>()
                };
            case QuestionType.Choice:
                return new ChoiceQuestionAnswer
                {
                    QuestionCode = questionCodeToken.ToString(),
                    Answer = answerToken.ToString()
                };
            default:
                throw new ApplicationException($"Unexpected question type {typeName}");
        }
    }
}
