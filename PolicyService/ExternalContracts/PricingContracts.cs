using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PricingService.Api.Commands
{
    public class CalculatePriceCommand
    {
        public string ProductCode { get; set; } = string.Empty;
        public DateTimeOffset PolicyFrom { get; set; }
        public DateTimeOffset PolicyTo { get; set; }
        public List<string>? SelectedCovers { get; set; }
        public List<PricingService.Api.Commands.Dto.QuestionAnswer>? Answers { get; set; }
    }

    public class CalculatePriceResult
    {
        public decimal TotalPrice { get; set; }
        public Dictionary<string, decimal> CoverPrices { get; set; } = new();
    }
}

namespace PricingService.Api.Commands.Dto
{
    [JsonConverter(typeof(PricingService.Api.Converters.QuestionAnswerConverter))]
    public abstract class QuestionAnswer
    {
        public string QuestionCode { get; set; } = string.Empty;
        public abstract QuestionType QuestionType { get; }
        public abstract object GetAnswer();
    }

    public enum QuestionType
    {
        Text,
        Number,
        Choice
    }

    public abstract class QuestionAnswer<T> : QuestionAnswer
    {
        public T Answer { get; set; } = default!;

        public override object GetAnswer()
        {
            return Answer!;
        }
    }

    public class TextQuestionAnswer : QuestionAnswer<string>
    {
        public override QuestionType QuestionType => QuestionType.Text;
    }

    public class NumericQuestionAnswer : QuestionAnswer<decimal>
    {
        public override QuestionType QuestionType => QuestionType.Number;
    }

    public class ChoiceQuestionAnswer : QuestionAnswer<string>
    {
        public override QuestionType QuestionType => QuestionType.Choice;
    }
}

namespace PricingService.Api.Converters
{
    internal class QuestionAnswerConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType.IsAssignableFrom(typeof(PricingService.Api.Commands.Dto.QuestionAnswer));
        }

        public override object ReadJson(
            JsonReader reader,
            Type objectType,
            object? existingValue,
            JsonSerializer serializer)
        {
            var jsonObject = JObject.Load(reader);
            var target = Create(jsonObject);
            serializer.Populate(jsonObject.CreateReader(), target);
            return target;
        }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            if (value is PricingService.Api.Commands.Dto.QuestionAnswer questionAnswer)
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

        private static PricingService.Api.Commands.Dto.QuestionAnswer Create(JObject jsonObject)
        {
            var questionTypeToken = jsonObject["questionType"]
                ?? throw new ApplicationException("questionType is required.");
            var questionCodeToken = jsonObject["questionCode"]
                ?? throw new ApplicationException("questionCode is required.");
            var answerToken = jsonObject["answer"]
                ?? throw new ApplicationException("answer is required.");

            var typeName = Enum.Parse<PricingService.Api.Commands.Dto.QuestionType>(questionTypeToken.ToString());

            return typeName switch
            {
                PricingService.Api.Commands.Dto.QuestionType.Text => new PricingService.Api.Commands.Dto.TextQuestionAnswer
                {
                    QuestionCode = questionCodeToken.ToString(),
                    Answer = answerToken.ToString()
                },
                PricingService.Api.Commands.Dto.QuestionType.Number => new PricingService.Api.Commands.Dto.NumericQuestionAnswer
                {
                    QuestionCode = questionCodeToken.ToString(),
                    Answer = answerToken.Value<decimal>()
                },
                PricingService.Api.Commands.Dto.QuestionType.Choice => new PricingService.Api.Commands.Dto.ChoiceQuestionAnswer
                {
                    QuestionCode = questionCodeToken.ToString(),
                    Answer = answerToken.ToString()
                },
                _ => throw new ApplicationException($"Unexpected question type {typeName}")
            };
        }
    }
}
