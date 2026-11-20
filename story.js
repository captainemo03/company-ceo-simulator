const storyScenarios={
 normal:{title:'Dengeli piyasa',status:'SERBEST OYUN',cls:'neutral',copy:'Piyasa koşulları normal. Şirketinizin iç kararları sonucu belirleyecek.',phases:[]},
 global_crisis:{title:'Küresel ekonomik kriz',status:'KRİZ AKTİF',cls:'down',copy:'Talep, finansman ve tüketici güveni aynı anda baskı altında. Krizi yönetmek için her ay farklı bir öncelik belirleyin.',phases:[
  {title:'Tüketici güveni daralıyor',copy:'Hanehalkı ve kurumlar harcamalarını erteliyor.',d:1,c:4,f:2,cost:7000,rep:-1},
  {title:'Kredi piyasası kilitleniyor',copy:'Bankalar ve yatırımcılar risk iştahını düşürüyor.',d:1,c:4,f:1,cost:9000,rep:-1},
  {title:'Maliyetler yapışkan kalıyor',copy:'Talep toparlanmadan önce operasyon maliyetleri baskı yaratıyor.',d:2,c:5,f:1,cost:11000,rep:-1}
 ]},
 supply_shock:{title:'Enerji ve tedarik krizi',status:'TEDARİK ŞOKU',cls:'down',copy:'Lojistik ve enerji maliyetleri yükseliyor. Stok, fiyatlama ve tedarikçi seçimi kritikleşiyor.',phases:[
  {title:'Enerji maliyetlerinde sıçrama',copy:'Operasyon ve teslimat giderleri yükseliyor.',d:3,c:5,f:3,cost:10000,rep:0},
  {title:'Teslimat süreleri uzuyor',copy:'Gecikmeler müşteri beklentilerini zorluyor.',d:2,c:5,f:3,cost:13000,rep:-1},
  {title:'Tedarikçi pazarlığı dönemi',copy:'Alternatif kaynak bulma maliyetli fakat risk azaltıcı.',d:3,c:4,f:3,cost:7000,rep:0}
 ]},
 capital_crunch:{title:'Fon piyasası güven krizi',status:'FİNANSMAN KRİZİ',cls:'down',copy:'Yatırımcılar büyümeden çok kârlılığa ve nakit disiplinine bakıyor.',phases:[
  {title:'Yatırımcı iştahı düşüyor',copy:'Yeni finansman turları daha seçici hale geliyor.',d:2,c:3,f:1,cost:4000,rep:-1},
  {title:'Değerleme baskısı artıyor',copy:'Şirketler daha düşük değerleme ve uzun vade ile karşılaşıyor.',d:2,c:3,f:1,cost:5000,rep:0},
  {title:'Kârlılık testi',copy:'Pazar, sürdürülebilir gelir modeli kanıtı istiyor.',d:3,c:3,f:2,cost:3000,rep:1}
 ]},
 growth:{title:'Küresel talep canlanması',status:'FIRSAT DÖNEMİ',cls:'up',copy:'Talep canlı; ancak aşırı yatırım ve kapasite hataları büyümeyi riske atabilir.',phases:[
  {title:'Yeni talep dalgası',copy:'Pazar daha hızlı teslimat ve güçlü ürün deneyimi bekliyor.',d:5,c:3,f:4,cost:0,rep:2},
  {title:'Kapasite yarışı',copy:'Rakipler büyüme yatırımlarını hızlandırıyor.',d:5,c:4,f:4,cost:3000,rep:1},
  {title:'Kalite eşiği',copy:'Hızla büyüyen şirketlerde müşteri desteği belirleyici hale geliyor.',d:4,c:4,f:4,cost:4000,rep:0}
 ]}
};
function storyMacro(phase){return {id:'story',title:phase.title,tag:'SENARYO',cls:'down',copy:phase.copy,d:phase.d,c:phase.c,f:phase.f,cost:phase.cost,rep:phase.rep}}
function storyResponseMeta(id){return {defend:{copy:'Kısa vadede ₺7.000 kaynak korunur; görünürlük ve itibar sınırlı azalır.'},value:{copy:'Müşteri güveni için ₺6.000 telafi harcanır; itibar ve müşteri bağlılığı desteklenir.'},protect:{copy:'Kritik ürün yatırımı için ₺9.000 ayrılır; kalite artar fakat kasa daralır.'}}[id]}
function syncStory(){if(!game)return;const scenario=storyScenarios[game.scenario]||storyScenarios.normal,phase=scenario.phases[Math.min(game.month-1,scenario.phases.length-1)],active=!!phase&&game.month<=scenario.phases.length;$('storyTitle').textContent=scenario.title;$('storyStatus').textContent=active?scenario.status:scenario.phases.length?'TOPARLANMA AŞAMASI':scenario.status;$('storyStatus').className='tag '+(active?scenario.cls:'neutral');$('storyCopy').textContent=active?phase.copy:scenario.copy;const total=Math.max(scenario.phases.length,3),activeIndex=active?game.month-1:-1,doneCount=active?activeIndex:scenario.phases.length;$('storyTimeline').innerHTML=Array.from({length:total},(_,i)=>`<span class="${i<doneCount?'done':i===activeIndex?'active':''}">${i<scenario.phases.length?scenario.phases[i].title:'Toparlanma'}</span>`).join('');$('storyResponseCopy').textContent=storyResponseMeta($('storyResponse').value).copy}
const storyRender=render;render=()=>{storyRender();syncStory()};
const storyAdvance=advance;advance=()=>{if(!game||game.ended)return;const scenario=storyScenarios[game.scenario]||storyScenarios.normal,phase=scenario.phases[game.month-1],response=$('storyResponse').value;if(phase){game.economy=storyMacro(phase);const effects={defend:()=>{game.cash+=7000;game.reputation=Math.max(0,game.reputation-1);game.logs.unshift('Kriz kararı: Nakit savunması uygulandı; görünürlük sınırlı azaltıldı.')},value:()=>{game.cash-=6000;game.reputation=Math.min(100,game.reputation+3);game.customers+=Math.max(2,Math.round(game.customers*.03));game.logs.unshift('Kriz kararı: Müşteri güveni için telafi ve değer paketi uygulandı.')},protect:()=>{game.cash-=9000;game.quality=Math.min(100,game.quality+4);game.logs.unshift('Kriz kararı: Kritik ürün yatırımı korunarak kalite güçlendirildi.')}};effects[response]()}storyAdvance();render()};
$('storyResponse').oninput=syncStory;$('advanceBtn').onclick=advance;
