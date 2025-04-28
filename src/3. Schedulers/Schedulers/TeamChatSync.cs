//using Microsoft.Graph.Models;
//using WebApp.Infrastructure.Helpers;

//namespace Schedulers
//{
//    internal class TeamChatSync
//    {
//        public void TeamChat()
//        {
//            var graphHelper = new GraphHelper();
//            LLMProcessor objLLM = new LLMProcessor();
//            graphHelper.Authenticate(); // Authenticate before calling GetUsersAsync
//            var users = graphHelper.GetUsers().Result;
//            Console.Write(users.Count);
//            foreach (var user in users)
//            {
//                if (user.DisplayName != null && user.DisplayName.Equals("Synop Sandbox", StringComparison.OrdinalIgnoreCase))
//                {
//                    var msgs = graphHelper.GetProcessedMessagesByUserIdAsync(user.Id).Result;
//                    Console.WriteLine(msgs);

//                    if (msgs != null)
//                    {
//                        foreach (var msg in msgs)
//                        {
//                            var st = objLLM.ProcessWithLLM(msg).Result;
//                        }
//                    }
//                }
//                else
//                {
//                    Console.WriteLine(user.DisplayName);
//                }
//            }

//        }
//    }
//}
