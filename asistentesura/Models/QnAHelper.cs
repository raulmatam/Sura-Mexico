using Newtonsoft.Json;

namespace SimpleEchoBot.Models
{
    public class QnAHelper
    {
        [JsonProperty(PropertyName = "answer")]
        public string Answer { get; set; }

        [JsonProperty(PropertyName = "score")]
        public double Score { get; set; }

        public static string HandleResponse(string receivedValue)
        {
            var response = JsonConvert.DeserializeObject<Rootobject>(receivedValue);
            var selectedAnswer = response.answers[0].answer;
            return selectedAnswer;
        }
    }

    public class Rootobject
    {
        public Answer[] answers { get; set; }
    }

    public class Answer
    {
        public string answer { get; set; }
        public string[] questions { get; set; }
        public float score { get; set; }
    }
}