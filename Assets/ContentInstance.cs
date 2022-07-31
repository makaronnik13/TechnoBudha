using System.IO;
using UnityEngine;

public class ContentInstance
{
    public ContentType ContentType;
    public string Content;
    public Sprite Sprite;

    public ContentInstance(string content, ContentType contentType)
    {
        this.Content = content;
        this.ContentType = contentType;
        if (contentType == ContentType.Image)
        {
            byte[] bytes = File.ReadAllBytes(content);
            Texture2D texture = new Texture2D(1, 1);
            texture.LoadImage(bytes);
            Sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        }
    }
}

public enum ContentType
{
    Text,
    Image,
    Video
}