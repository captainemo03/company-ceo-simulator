const macroEvents=[
 {id:'steady',title:'Dengeli ekonomik görünüm',tag:'STABİL',cls:'neutral',copy:'Talep ve finansmana erişim normal seyrinde. Şirketinizin iç kararları daha belirleyici.',d:3,c:2,f:3,cost:0,rep:0},
 {id:'inflation',title:'Yüksek enflasyon dalgası',tag:'MALİYET RİSKİ',cls:'down',copy:'Girdi ve hizmet maliyetleri hızla artıyor. Fiyatlama ve kasa disiplini kritik.',d:2,c:5,f:2,cost:14000,rep:-1},
 {id:'currency',title:'Kur oynaklığı',tag:'DALGALI',cls:'down',copy:'Kur hareketleri ithal girdi ve reklam maliyetlerini belirsizleştiriyor. Riskten korunma etkili olabilir.',d:2,c:4,f:2,cost:18000,rep:0},
 {id:'fundTrust',title:'Fon piyasası güven krizi',tag:'GÜVEN ŞOKU',cls:'down',copy:'Yatırım araçlarına ilişkin güven sarsıldı. Müşteriler ve yatırımcılar daha temkinli davranıyor.',d:2,c:3,f:1,cost:7000,rep:-4},
 {id:'globalBoom',title:'Küresel talep canlanması',tag:'FIRSAT',cls:'up',copy:'Dış pazarlarda talep güçleniyor. Ürün kalitesi yüksek şirketler hızlı büyüyebilir.',d:5,c:2,f:4,cost:0,rep:4},
 {id:'creditTight',title:'Kredi muslukları sıkılaştı',tag:'KREDİ DARALMASI',cls:'down',copy:'Bankalar kredi koşullarını ağırlaştırdı. Mevcut nakdi verimli kullanmak daha önemli.',d:2,c:3,f:1,cost:4000,rep:-1}
];
const worldHeadlines=[
 'KÜRESEL PİYASALAR — Enerji ve lojistik maliyetleri şirketlerin marjlarını yeniden şekillendiriyor.',
 'DÜNYA TİCARETİ — Tedarik ağlarında bölgesel teslimat süreleri yakından izleniyor.',
 'TEKNOLOJİ GÜNDEMİ — Yapay zekâ yatırımları verimlilik yarışını hızlandırıyor.',
 'FİNANS MERKEZLERİ — Yatırımcılar kârlılık ve nakit disiplinine daha fazla ağırlık veriyor.',
 'KÜRESEL TALEP — Tüketici güveni sektörler arasında farklılaşmaya devam ediyor.'
];
let worldHeadlineIndex=0;
function dotsMacro(n){return '●'.repeat(n)+'○'.repeat(5-n)}
function tickerText(e){let world=worldHeadlines[worldHeadlineIndex%worldHeadlines.length];return `${world}  •  EKONOMİK BÜLTEN — ${e.title.toUpperCase()}: ${e.copy}  •  CEO NOTU — Kararlarınızı piyasa koşullarına göre güncelleyin.`}
function refreshWorldTicker(){worldHeadlineIndex++;let e=(typeof game!=='undefined'&&game?.economy)||macroEvents[0],headline=tickerText(e);$('tickerOne').textContent=headline;$('tickerTwo').textContent=headline}
refreshWorldTicker();setInterval(refreshWorldTicker,9000);
function randomMacro(){return macroEvents[Math.floor(Math.random()*macroEvents.length)]}
const renderWithMacro=render;
render=()=>{renderWithMacro();if(!game)return;game.economy=game.economy||macroEvents[0];let e=game.economy,headline=tickerText(e);$('macroTitle').textContent=e.title;$('macroTag').textContent=e.tag;$('macroTag').className='tag '+e.cls;$('macroCopy').textContent=e.copy;$('macroDemand').textContent=dotsMacro(e.d);$('macroCost').textContent=dotsMacro(e.c);$('macroCredit').textContent=dotsMacro(e.f);$('tickerOne').textContent=headline;$('tickerTwo').textContent=headline;let blocked=e.f===1;$('loanBtn').disabled=!!game.debt||blocked;$('loanBtn').textContent=blocked?'Kredi piyasası kapalı':game.debt?'Önce borcu kapatın':'₺100.000 büyüme kredisi al'};
const startWithMacro=$('startBtn').onclick;
$('startBtn').onclick=()=>{startWithMacro();game.economy=macroEvents[0];render()};
const advanceWithMacro=advance;
advance=()=>{if(!game||game.ended)return;let e=game.economy||macroEvents[0],action=$('cfoAction').value,impact=e.cost,rep=e.rep;if(action==='hedge'){game.cash-=10000;impact=Math.round(impact*.4);game.logs.unshift('CFO: riskten korunma planı uygulandı.')}if(action==='conserve'){game.reputation=Math.max(0,game.reputation-2);impact=Math.round(impact*.65);game.logs.unshift('CFO: kasa koruma modu büyümeyi yavaşlattı.')}if(impact){game.cash-=impact;game.logs.unshift('Ekonomik etki: '+e.title+' nedeniyle '+money(impact)+' ek maliyet oluştu.')}if(rep)game.reputation=Math.max(0,Math.min(100,game.reputation+rep));advanceWithMacro();game.economy=randomMacro();render()};
$('advanceBtn').onclick=advance;
