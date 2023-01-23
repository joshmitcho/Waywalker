using System.Collections.Generic;
using System.Xml.Serialization;

[XmlRoot("map")]
public class Tmx
{
  [XmlAttribute("width")] public int width;
  [XmlAttribute("height")] public int height;
  [XmlAttribute("tilewidth")] public int tileWidth;
  [XmlAttribute("tileheight")] public int tileHeight;
  [XmlElement("tileset")] public TmxTileset tileset;
  [XmlElement("layer")] public List<TmxLayer> layers;
}
public class TmxTileset
{
  [XmlAttribute("source")] public string source;
}
public class TmxLayer
{
  [XmlAttribute("width")] public int width;
  [XmlAttribute("height")] public int height;
  [XmlElement("data")] public TmxLayerData data;
}
public class TmxLayerData
{
  [XmlAttribute("encoding")] public string encoding;
  [XmlText] public string layerData;
}