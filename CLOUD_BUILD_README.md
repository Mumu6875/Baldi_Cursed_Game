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

## 1.5.0 — Mobil Run ve Look Back düğmeleri

- Sağ taraftaki Run düğmesi, verilen koşan bacak görseliyle; yeni Look Back
  düğmesi ise dikiz aynası görseliyle değiştirildi.
- Android'de parmak düğme sınırından biraz taşınca Run durumunu erken bırakan
  `PointerExit` davranışı kaldırıldı. Run artık parmak kaldırılana veya dokunuş
  iptal edilene kadar basılı kalır.
- Düğme olay bileşeni Unity'nin güvenilir biçimde oluşturabildiği üst seviye
  bir `MonoBehaviour` haline getirildi; görseller build öncesinde doğrulanır.
- Phase 2'de birinci defter sorularından biri yanlış cevaplandığında korku modu
  hemen etkinleşir.

## 1.5.1 — Phase 2 müzik yavaşlatması

- Phase 2 sırasında ana menünün `mus_Intro` parçası yarı hızda (`pitch 0.5`)
  çalar; Baldi'nin ana menü konuşması normal hızda kalır.
- Oyuna başlanınca çalan `schoolMusic` ve You Can Think Pad sırasında kullanılan
  `learnMusic` yalnızca Phase 2'de yarı hızda çalar.
- Müzik ayarı sahne yüklenir yüklenmez ve bir sonraki karede yeniden uygulanır;
  geç oluşturulan ses nesnelerinde ayarın kaçırılması engellenir.

## 1.5.2 — Unity NPOT düğme içe aktarma düzeltmesi

- Unity'nin 255x127 kaynak düğmeleri varsayılan NPOT ayarıyla 256x128 olarak
  içe aktarması artık geçerli kabul edilir.
- Build doğrulaması dosyanın varlığını, başarıyla `Texture2D` oluşmasını,
  oynanabilir minimum ölçüyü ve yaklaşık 2:1 en-boy oranını kontrol eder.
- Başarısız build günlüğündeki tek durdurucu hata olan gereksiz tam piksel
  eşitliği kaldırıldı; doğrulanan gerçek içe aktarma ölçüsü loga yazdırılır.

## 1.6.0 — Think Pad çarpma soruları

- Çözülebilir You Can Think Pad soruları artık toplama ve çıkarmaya ek olarak
  `0–9` aralığında çarpma işlemleri de üretir.
- Üç işlem eşit olasılıkla seçilir ve çarpma soruları mevcut `BAL_Math_Times`
  sesini kullanarak Baldi tarafından doğru sırada okunur.
- İmkânsız üçüncü soru ile Phase 1/Phase 2 yanlış cevap ve korku tetikleyicileri
  değiştirilmemiştir.

## 1.7.0 — Yeni yüksek menzilli Baldi cetvel sesi

- Özgün `BAL_Slap` klibi, verilen kayıt temel alınarak hazırlanan yeni mono OGG
  sesle çalışma zamanında değiştirilir; tüm Baldi sahnelerinde aynı ses kullanılır.
- Kayıttaki yaklaşık 0,85 saniyelik başlangıç sessizliği kaldırıldı; dinamik
  sıkıştırma ve limiter ile ortalama seviye eski cetvel klibinden yaklaşık
  1,2 dB daha yüksek hale getirildi.
- Baldi ses kaynağının tam ses mesafesi en az 12 birime, azami işitme menzili
  500 birime çıkarıldı ve ses tamamen 3B konumsal hale getirildi.
- Build öncesi doğrulama, yeni sesin mevcut ve geçerli bir mono `AudioClip`
  olduğunu kontrol eder.

## 1.7.1 — Cetvel sesi taşma düzeltmesi

- Önceki OGG'nin çözümleme sırasında `+2,5 dBTP` seviyesine çıkarak Android'de
  distorsiyon oluşturma riski giderildi.
- Ses yeniden sıkıştırılıp gerçek dosya üzerinde ölçüldü: yaklaşık `-18,7 LUFS`
  bütünleşik ses yüksekliği ve `-0,8 dBTP` güvenli gerçek tepe seviyesi.
- Kaynak telefon kaydına göre yaklaşık 15,5 LU daha yüksek olan sesin 500 birimlik
  3B işitme menzili korunur.

## 1.7.2 — Tek vuruş ses iyileştirmesi

- Uzun ve birden fazla adım içeren kayıt, Baldi hızlandığında seslerin üst üste
  binmemesi için en temiz tek vuruşa indirildi (`0,57 sn`, mono, `48 kHz`).
- Telefon kaydına hafif gürültü temizleme, dip gürültüsü filtresi, gövde/atak EQ'su,
  yumuşak kompresyon ve giriş/çıkış geçişleri uygulandı.
- Son OGG dosyası çözülmüş haliyle yaklaşık `-16,0 LUFS` ve `-2,1 dBTP` ölçülür;
  bu değer güçlü duyulurken dijital taşma payı bırakır.
- Build doğrulaması sesin mono ve `0,5–1,0 sn` arasında olduğunu kontrol eder.
  Baldi'nin mevcut 12/500 birimlik 3B ses mesafeleri değiştirilmemiştir.

## 1.8.0 — Mobil joystick ve ikon yenilemesi

- Sol joystick'in dokunma alanı `220x220` yerine `320x320` yapıldı; düğme
  `118x118` boyutuna çıkarıldı ve dairesel kırmızı görsel katmanlar eklendi.
- `0,12` merkez ölü bölgesi ve ölü bölge sonrası yeniden ölçekleme ile küçük
  parmak titreşimleri engellenirken tam hareket hızı korunur.
- Joystick artık yalnızca ilk dokunan parmağı takip eder; başka bir parmakla
  kamera veya aksiyon butonları kullanıldığında yön aniden değişmez.
- Run ve Look Back yazıları görsellerden tamamen kaldırıldı. Yerlerine şeffaf
  zeminli koşma ve dikiz aynası ikonları, `150x150` dairesel dokunma alanlarında
  kullanılır.
- Build doğrulaması iki mobil ikonun da kare ve en az `240x240` olduğunu kontrol eder.

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
- Android minimum API seviyesi 26 ve uygulama sürümü 1.8.0 olarak sabitlendi.

## Lisans / dağıtım

Temel topluluk projesinin koşulları gereği yalnızca ticari olmayan kullanım
içindir. Mystman12 / Basically Games ve kullanılan açık kaynak aracını
kredilerde belirtin. Baldi's Basics markası ve özgün içerik hakları sahiplerine
aittir.
