using System;

[Serializable]
public class BuddaSettings
{
    public string TextsPath = "Content/Texts.txt";
    public string ImagesPath = "Content/Images";
    public string VideosPath = "Content/Videos";

    public float FillTime = 2f;
    public float UnFillTime = 1.5f;

    public float TextChance = 1f;
    public float ImageChance = 1f;
    public float VideoChance = 1f;
}