using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace GazeDataViewer.Classes.Serialization
{
    public static class SerializationHelper
    {
        public static void SerializeToFile(string path,  SpotEyeTrackState spotEyeTrackState)
        {
            var serializer = new XmlSerializer(spotEyeTrackState.GetType());
            using (var writer = XmlWriter.Create(path))
            {
                serializer.Serialize(writer, spotEyeTrackState);
            }
        }

        public static SpotEyeTrackState DeserializeFromFile(string path)
        {
            SpotEyeTrackState result = null;
            var serializer = new XmlSerializer(typeof(SpotEyeTrackState));
            using (var reader = XmlReader.Create(path))
            {
                result = (SpotEyeTrackState)serializer.Deserialize(reader);
            }

            return result;
        }



       
     
    }
}
