const goalMeta={
  scale:{title:'Pazar payı',copy:'1.000 aktif müşteriye ulaşarak sektörde kalıcı yer edinin.',value:g=>g.customers,target:1000,win:'Pazar payı hedefini aştınız!'},
  profit:{title:'Karlılık',copy:'Aylık ₺100.000 kâr üreterek sürdürülebilir iş modelini kanıtlayın.',value:g=>Math.max(0,g.last),target:100000,win:'Karlılık hedefini aştınız!'},
  brand:{title:'Marka',copy:'Ürün ve müşteri deneyimiyle itibarı 90 puana çıkarın.',value:g=>g.reputation,target:90,win:'Marka hedefini aştınız!'}
};
function hasWon(g){let m=goalMeta[g.goal||'scale'];return m.value(g)>=m.target}
const renderWithGoals=render;
render=()=>{renderWithGoals();if(!game)return;game.goal=game.goal||'scale';let m=goalMeta[game.goal],value=Math.min(m.target,m.value(game)),pct=Math.round(value/m.target*100);$('goalTitle').textContent=m.title;$('goalCopy').textContent=m.copy;$('goalStatus').textContent=pct+'%';$('goalProgress').style.width=pct+'%'};
const reportWithGoals=showReport;
showReport=(profit,newCustomers,costs,event)=>{let won=hasWon(game);reportWithGoals(profit,newCustomers,costs,event);if(won&&!game.goalMilestone){let m=goalMeta[game.goal];game.goalMilestone=true;game.logs.unshift('Stratejik hedef tamamlandı: '+m.title+'. Şirket faaliyetlerine devam ediyor.');$('modalIcon').textContent='★';$('modalEyebrow').textContent='HEDEF DÖNÜM NOKTASI';$('modalTitle').textContent=m.win;$('modalText').textContent='Bu bir oyun sonu değil. Şirketiniz artık daha büyük hedefler, yıllık kurul puanı ve hissedar beklentileriyle yönetiliyor.';$('continueBtn').textContent='Şirketi yönetmeye devam et'}};
const startWithGoal=start;
$('startBtn').onclick=()=>{startWithGoal();game.goal=$('goal').value;render()};
['product','marketing'].forEach(id=>$(id).oninput=render);
