using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
namespace Scribe.Connector.Test
{
    public static class JsonDataFileHelper
    {
        public static T GetObjectFromDisk<T>(string fileName)
        {
            //C:\\_Git\\etouches\\Scribe.Connector.Test\\bin\\Debug\\
            //C:\_Git\etouches\Scribe.Connector.Test\TestData
            string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), fileName);
            path = path.Replace("\\bin\\Debug\\", "\\TestData\\");
            string data = File.ReadAllText(path);
            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(data);
        }
        

    }
}
