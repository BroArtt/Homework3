using Homework3;
using Newtonsoft.Json;
using System.Net;
using System.Text;
using XSystem.Security.Cryptography;

namespace HttpListenerBrovko
{
    internal class Program
    {
        static void Main(string[] args)
        {
            HttpListener listener = new HttpListener();
            listener.Prefixes.Add("http://localhost:8888/");
            listener.Start();
            Console.WriteLine("Ожидание подключений...");

            while (true)
            {
                HttpListenerContext context = listener.GetContext();
                HttpListenerRequest request = context.Request;
                HttpListenerResponse response = context.Response;
                Stream body = request.InputStream;
                Encoding encoding = request.ContentEncoding;
                StreamReader reader = new StreamReader(body, encoding);
                var Salt = Guid.NewGuid().ToString();
                var userName = string.Empty;
                var password = string.Empty;
                var contentToUpload = string.Empty;
                var streamRead = string.Empty;
                var FilePath = $"../../../Users/";
                streamRead = reader.ReadToEnd();
                var json = JsonConvert.DeserializeObject<User>(streamRead);

                if (request.HttpMethod=="POST" && request.Url.AbsolutePath=="/sign-up" && !File.Exists($"{FilePath}{json.UserName}.txt"))
                {
                    response.StatusCode = 200;
                    userName = json.UserName;
                    password = json.Password;
                    var HashPassandSalt = HashFunc(password, Salt);
                    FilePath = $"{FilePath}{userName}.txt";
                    var FileString = $"{HashPassandSalt},{Salt}";
                    File.WriteAllText(FilePath, FileString);
                    contentToUpload = "Registration is successful";
                    
                }
                else if(File.Exists($"{FilePath}{json.UserName}.txt"))
                {
                    contentToUpload = "User already exists";
                }else {
                    response.StatusCode = 400;
                    contentToUpload = "Bad request";
                }

                byte[] bufferhtml = Encoding.UTF8.GetBytes(contentToUpload);

                response.ContentLength64 = bufferhtml.Length;
                Stream output = response.OutputStream;
                output.Write(bufferhtml, 0, bufferhtml.Length);
                output.Close();

            }
            string HashFunc(string password, string Salt)
            {
                password = $"{password}{Salt}";
                byte[] bytepass = Encoding.ASCII.GetBytes(password);
                var tmpHash = new MD5CryptoServiceProvider().ComputeHash(bytepass);
                return ByteArrayToString(tmpHash);
            }
            static string ByteArrayToString(byte[] arrInput)
            {
                int i;
                StringBuilder sOutput = new StringBuilder(arrInput.Length);
                for (i = 0; i < arrInput.Length; i++)
                {
                    sOutput.Append(arrInput[i].ToString("X2"));
                }
                return sOutput.ToString();
            }


        }
    }
}