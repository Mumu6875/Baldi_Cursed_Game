# Baldi Cursed Classroom — Unity Build Automation adayı

Bu klasör, özgün Unity 2018.3.9f1 mod projesinin **ayrı** bir Unity
Unity 6000.5.9f1 (Unity 6.5) büyük-sürüm geçiş adayıdır. Özgün 2018.3
projesi değiştirilmemiştir.

## Unity Build Automation ayarları

- Unity sürümü: `6000.5.9f1`
- Platform: Android
- Çıktı: APK (Google Play için AAB seçilebilir)
- Minimum Android API: 26
- Mimariler: ARMv7 + ARM64
- Build scenes: `ProjectSettings/EditorBuildSettings.asset` içindeki etkin sahneler
- Test APK'sı: otomatik debug anahtarı kullanılabilir
- Yayın: kendi release keystore dosyanızı Unity Dashboard'da tanımlayın

## 1.1.0 — Room 99 final sekansı

- Dördüncü/final çıkış artık sonuç sahnesini hemen açmaz ve kilitlenir.
- Okulun ışığı çok azalır, yoğun beyaz sis başlar ve NPC'ler geçici olarak kaybolur.
- Sahnedeki özgün `99` materyaliyle işaretlenen mevcut Room 99 kapısı bulunur.
- Room 99 kapısından geçmek, çalışma anında oluşturulan 11x11 karanlık labirente götürür.
- Labirentin en derinindeki beyaz sis çekirdeğine girildiğinde oyun kapanır.

## Önemli dönüşüm notu

Bu paket Unity 6.5 geçiş adayıdır. Eski Analytics, Package Manager UI,
TextMesh Pro 1.3 ve kaldırılmış yerleşik modül bağımlılıkları temizlenmiş;
Unity 6.5 ile gelen uGUI 2.6.0/TMP birleşimine geçirilmiştir. Bu çalışma
ortamında Unity Editor bulunmadığı için ilk Asset Database yeniden içe aktarma
ve sahne yeniden serileştirme işlemi yerelde çalıştırılamadı. Build Automation
ilk içe aktarmada bir C# veya varlık dönüşüm hatası verirse gerçek logdaki ilk
hata düzeltilip yeni bir geçiş turu yapılmalıdır.

## Lisans / dağıtım

Temel topluluk projesinin koşulları gereği yalnızca ticari olmayan kullanım
içindir. Mystman12 / Basically Games ve kullanılan açık kaynak aracını
kredilerde belirtin. Baldi's Basics markası ve özgün içerik hakları sahiplerine
aittir.
