# OpenCV Yüz Tespiti

C# ve OpenCvSharp ile çalışan bir konsol uygulaması. `image.png` içindeki yüzleri Haar cascade ile bulur, kırmızı dikdörtgenle işaretler ve sonucu `result.jpg` olarak kaydeder.

Önden bakış, sağ/sol profil ve örtüşen kutular için ek filtreler kullanır.

## Gereksinimler

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- Windows (proje `OpenCvSharp4.runtime.win` kullanır)
- Visual C++ Redistributable (OpenCvSharp native DLL’leri için)

macOS veya Linux’ta çalıştırmak için ilgili `OpenCvSharp4.runtime.*` paketini `opencvp.csproj` dosyasına eklemeniz gerekir.

## Çalıştırma

```powershell
cd opencvp
dotnet run
```

Başarılı çalışınca konsolda tespit edilen yüz sayısı ve çıktı yolu görünür:

```text
Tespit edilen yüz sayısı: 5
İşlem tamamlandı.
Sonuç: ...\opencvp\bin\Debug\net10.0\result.jpg
```

Farklı bir görsel denemek için `opencvp/image.png` dosyasını değiştirin. Uygulama çıktı klasörüne kopyalanan `image.png` dosyasını okur.

## Nasıl çalışır

1. Görseli okur ve gri tonlamaya çevirir.
2. CLAHE ile kontrastı artırır (düşük ışık / gölge için).
3. `haarcascade_frontalface_default.xml` ile önden yüz arar.
4. `haarcascade_profileface.xml` ile profil yüz arar; ayna görüntüsünde de arayıp koordinatları geri çevirir.
5. Küçük, orantısız veya çok örtüşen kutuları eler.
6. Kalan yüzlerin etrafına kırmızı dikdörtgen çizer ve `result.jpg` yazar.

## Proje yapısı

```text
OpenCVproject/
├── opencv.slnx
├── opencvp/
│   ├── Program.cs
│   ├── opencvp.csproj
│   ├── image.png
│   ├── haarcascade_frontalface_default.xml
│   └── haarcascade_profileface.xml
└── README.md
```

## Bağımlılıklar

| Paket | Rol |
| --- | --- |
| `OpenCvSharp4` | OpenCV C# sarmalayıcısı |
| `OpenCvSharp4.runtime.win` | Windows native OpenCV DLL’leri (`OpenCvSharpExtern`) |

Yalnızca managed paketi eklemek `DllNotFoundException` üretir. Windows’ta runtime paketinin de projede olması gerekir.
