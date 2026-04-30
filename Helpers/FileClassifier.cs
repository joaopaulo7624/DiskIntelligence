using System.Collections.Frozen;

namespace DiskIntelligence.Helpers;

public static class FileClassifier
{
    private static readonly FrozenDictionary<string, string> _extensionMap = new Dictionary<string, string>
    {
        ["mp4"] = "Video", ["mkv"] = "Video", ["avi"] = "Video", ["mov"] = "Video", ["wmv"] = "Video",
        ["flv"] = "Video", ["webm"] = "Video", ["m4v"] = "Video", ["3gp"] = "Video",

        ["mp3"] = "Audio", ["wav"] = "Audio", ["flac"] = "Audio", ["aac"] = "Audio", ["ogg"] = "Audio",
        ["wma"] = "Audio", ["m4a"] = "Audio", ["opus"] = "Audio", ["aiff"] = "Audio",

        ["jpg"] = "Imagem", ["jpeg"] = "Imagem", ["png"] = "Imagem", ["gif"] = "Imagem", ["bmp"] = "Imagem",
        ["svg"] = "Imagem", ["webp"] = "Imagem", ["ico"] = "Imagem", ["tiff"] = "Imagem",
        ["heic"] = "Imagem", ["raw"] = "Imagem", ["cr2"] = "Imagem", ["nef"] = "Imagem",

        ["pdf"] = "Documento", ["doc"] = "Documento", ["docx"] = "Documento", ["xls"] = "Documento",
        ["xlsx"] = "Documento", ["ppt"] = "Documento", ["pptx"] = "Documento", ["txt"] = "Documento",
        ["odt"] = "Documento", ["ods"] = "Documento", ["odp"] = "Documento", ["rtf"] = "Documento",
        ["md"] = "Documento", ["csv"] = "Documento", ["epub"] = "Documento", ["mobi"] = "Documento",

        ["zip"] = "Arquivo", ["rar"] = "Arquivo", ["7z"] = "Arquivo", ["tar"] = "Arquivo", ["gz"] = "Arquivo",
        ["bz2"] = "Arquivo", ["xz"] = "Arquivo", ["zst"] = "Arquivo", ["iso"] = "Arquivo", ["dmg"] = "Arquivo",

        ["rs"] = "Codigo", ["js"] = "Codigo", ["ts"] = "Codigo", ["jsx"] = "Codigo", ["tsx"] = "Codigo",
        ["py"] = "Codigo", ["java"] = "Codigo", ["cpp"] = "Codigo", ["c"] = "Codigo", ["h"] = "Codigo",
        ["cs"] = "Codigo", ["go"] = "Codigo", ["rb"] = "Codigo", ["php"] = "Codigo", ["html"] = "Codigo",
        ["css"] = "Codigo", ["scss"] = "Codigo", ["json"] = "Codigo", ["toml"] = "Codigo",
        ["yaml"] = "Codigo", ["yml"] = "Codigo", ["xml"] = "Codigo", ["sh"] = "Codigo",
        ["ps1"] = "Codigo", ["bat"] = "Codigo", ["lua"] = "Codigo", ["swift"] = "Codigo", ["kt"] = "Codigo",

        ["exe"] = "Executavel", ["msi"] = "Executavel", ["dll"] = "Executavel", ["sys"] = "Executavel",
        ["apk"] = "Executavel", ["appx"] = "Executavel", ["app"] = "Executavel", ["deb"] = "Executavel",
        ["rpm"] = "Executavel",

        ["tmp"] = "TempLixo", ["log"] = "TempLixo", ["bak"] = "TempLixo", ["cache"] = "TempLixo",
        ["old"] = "TempLixo", ["temp"] = "TempLixo", ["crdownload"] = "TempLixo", ["part"] = "TempLixo",

        ["ttf"] = "Fonte", ["otf"] = "Fonte", ["woff"] = "Fonte", ["woff2"] = "Fonte", ["eot"] = "Fonte",
    }.ToFrozenDictionary();

    public static string Classify(string extension)
    {
        return _extensionMap.TryGetValue(extension.ToLowerInvariant(), out var cat) ? cat : "Outros";
    }

    public static (string Color, string Icon) GetCategoryMeta(string category)
    {
        return category switch
        {
            "Video" => ("#ff6b35", "\U0001f3ac"),
            "Audio" => ("#a855f7", "\U0001f3b5"),
            "Imagem" => ("#f59e0b", "\U0001f5bc"),
            "Documento" => ("#22c55e", "\U0001f4c4"),
            "Arquivo" => ("#f97316", "\U0001f4e6"),
            "Codigo" => ("#3b82f6", "\U0001f4bb"),
            "Executavel" => ("#ef4444", "\u2699"),
            "TempLixo" => ("#6b7280", "\U0001f5d1"),
            "Fonte" => ("#ec4899", "\U0001f524"),
            _ => ("#94a3b8", "\U0001f4c1"),
        };
    }
}