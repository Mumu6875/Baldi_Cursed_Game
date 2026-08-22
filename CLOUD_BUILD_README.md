# Baldi Cursed Classroom — Unity Build Automation adayı

Bu klasör, özgün Unity 2018.3.9f1 mod projesinin **ayrı** bir Unity
2019.4.41f1 bulut-derleme kopyasıdır. Özgün 2018.3 projesi değiştirilmemiştir.

## Unity Build Automation ayarları

- Unity sürümü: `2019.4.41f1` (CVE-2025-59489 düzeltmeli sürüm)
- Platform: Android
- Çıktı: APK (Google Play için AAB seçilebilir)
- Minimum Android API: 23
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

Proje ilk kez Unity 2019.4.41f1 ile açıldığında Unity varlıkları ve proje
ayarlarını 2018.3 biçiminden 2019.4 biçimine içe aktarır. Bu çalışma ortamında
Unity Editor bulunmadığı için bu ilk içe aktarma yerelde çalıştırılamadı.
Build Automation içe aktarma sırasında derleme hatası verirse projeyi bir kez
Unity 2019.4.41f1 ile açın, Console'daki ilk hatayı düzeltin, kaydedin ve depoya
yeniden gönderin.

## Lisans / dağıtım

Temel topluluk projesinin koşulları gereği yalnızca ticari olmayan kullanım
içindir. Mystman12 / Basically Games ve kullanılan açık kaynak aracını
kredilerde belirtin. Baldi's Basics markası ve özgün içerik hakları sahiplerine
aittir.
