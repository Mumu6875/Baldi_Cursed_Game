# Baldi Cursed Classroom — Unity Build Automation adayı

Bu klasör, özgün Unity 2018.3.9f1 mod projesinin **ayrı** bir
Unity 6000.3.22f1 (Unity 6.3 LTS) büyük-sürüm geçiş adayıdır. Özgün 2018.3
projesi değiştirilmemiştir.

## Unity Build Automation ayarları

- Unity sürümü: `6000.3.22f1` (Unity Build Automation destekli LTS)
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

## 1.3.0 — Mobil kontrol ve korku geçişi

- Android kamera kaydırması tek dokunuş kaynağına sabitlendi; çift/ani dönüşler
  engellendi ve hassasiyet azaltıldı.
- Duraklatma dokunuşu kısa bir giriş darbesiyle güvenilir hale getirildi; buton
  üçüncü eşya yuvasından ekranın üst ortasına taşındı.
- Oyun normal ışıkla, normal Baldi ve normal Think Pad görünümüyle başlar.
- İkinci defterin son sorusu gönderildiğinde korku aşaması etkinleşir: ortam
  kararır ve Baldi ile Think Pad lanetli görünüme geçer.
- Korku ve Room 99 aydınlatması, oynanabilirliği korumak için önceki sürümden
  daha aydınlık ayarlandı.

## 1.4.0 — Phase 1 / Phase 2

- İlk kurulum Phase 1 olarak başlar. İkinci defterin üçüncü cevabı gönderilince
  tam ekran sahte korsan-sürüm uyarısı gösterilir.
- Uyarıya dokunulduğunda Phase 2 kalıcı olarak kaydedilir ve Android uygulaması
  kapanır.
- Oyun yeniden açıldığında Phase 2 etkindir. Bu kez ikinci defterin üçüncü
  cevabı gönderildiğinde uyarı yerine lanetli korku aşaması başlar.

## 1.4.1 — Android kamera düzeltmesi

- Bazı Android cihazlarda sıfır dönen EventSystem sürükleme farkı yerine parmak
  ekran konumları doğrudan karşılaştırılır.
- Sürükleme eşiği kapatıldı; kamera küçük kaydırmalara hemen yanıt verir.
- Dönüş hızı oynanabilir seviyeye yükseltilirken ani sıçrama sınırı korunur.

## 1.4.2 — Ham Android Touch kamera girişi

- Kamera, cihazdan olay alamayan UI/EventSystem sürükleme katmanından ayrıldı.
- Sağ taraftaki kamera alanı doğrudan `Input.touchCount` ve `Input.GetTouch`
  kullanarak Android parmak konumlarını her karede okur.
- Oyuncu kamera kodunda dokunmadan üretilen fare ekseni kullanılmaz; çift giriş
  engellenirken eski UI tıklama uyumluluğu korunur.
- Eşya HUD'ı ile RUN/GRAB/USE/PAUSE alanları kamera dokunuşundan hariç tutuldu.

## 1.4.3 — Notebook klavyesi ve erken Phase 1 uyarısı

- Notebook cevap alanı prefab ve çalışma zamanı seviyesinde etkileşimsiz/salt
  okunur yapıldı; Android yazılım klavyesi artık çağrılmaz.
- Phase 1 sırasında birinci defterde yanlış cevap verilmesi korsan uyarısını
  hemen gösterir. İkinci defterin son cevabı alternatif tetikleyici olarak kalır.
- Önceki test kurulumlarındaki Phase 2 kaydından bağımsız yeni bir durum anahtarı
  kullanılır; bu sürüm ilk açılışta yenilenmiş Phase 1'i bir kez gösterir.
- Build öncesi doğrulama, 1672x941 uyarı görselinin Resources içinde gerçekten
  bulunmasını zorunlu kılar; eksik görselle APK üretilemez.

## 1.4.4 — Bulut görsel içe aktarma düzeltmesi

- GitHub aktarımında kesilerek bozulmuş büyük PNG kaldırıldı.
- Aynı 1672x941 uyarı görseli, tamlığı doğrulanmış 325 KB yüksek kaliteli JPEG
  olarak eklendi; metin ve görünüm korunur.
- Build doğrulayıcı dosyayı eşzamanlı yeniden içe aktarır ve hem fiziksel dosyayı
  hem de oluşan Unity `Texture2D` varlığını kontrol eder.

## Önemli dönüşüm notu

Bu paket Unity 6.3 LTS geçiş adayıdır. Eski Analytics, Package Manager UI,
TextMesh Pro 1.3 ve kaldırılmış yerleşik modül bağımlılıkları temizlenmiş;
Unity 6 ile gelen uGUI 2.0.0/TMP birleşimine geçirilmiştir. Bu çalışma
ortamında Unity Editor bulunmadığı için ilk Asset Database yeniden içe aktarma
ve sahne yeniden serileştirme işlemi yerelde çalıştırılamadı. Build Automation
ilk içe aktarmada bir C# veya varlık dönüşüm hatası verirse gerçek logdaki ilk
hata düzeltilip yeni bir geçiş turu yapılmalıdır.

## Unity 6.3 bulut düzeltmeleri

- Eski ve kullanılmayan TMP örnek script derleme hataları kaldırıldı.
- TMP ayar kaynağı `assetVersion: 2` olarak işaretlendi; batch mode sırasında
  grafik arayüzlü TMP kaynak içe aktarma penceresi artık açılmaz.
- Unity 6'da nondeterministic import hatası oluşturan eski TMP sprite
  kaynakları kaldırıldı.
- Android minimum API seviyesi 26 ve uygulama sürümü 1.4.4 olarak sabitlendi.

## Lisans / dağıtım

Temel topluluk projesinin koşulları gereği yalnızca ticari olmayan kullanım
içindir. Mystman12 / Basically Games ve kullanılan açık kaynak aracını
kredilerde belirtin. Baldi's Basics markası ve özgün içerik hakları sahiplerine
aittir.
