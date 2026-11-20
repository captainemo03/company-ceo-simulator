(function(){
  'use strict';
  const E=window.BusinessEmpireEngine;
  if(!E)return;
  const clamp=(n,a=0,b=100)=>Math.max(a,Math.min(b,Number(n)||0));
  const currentGame=()=>{try{return game||null}catch{return null}};
  const segmentSets={
    saas:[
      {id:'low_end',name:'Low End',size:.22,growth:.035,price:72,weights:{price:.28,performance:.08,quality:.12,reliability:.1,brand:.08,age:.06,fit:.12,access:.16}},
      {id:'traditional',name:'Traditional',size:.26,growth:.025,price:100,weights:{price:.18,performance:.12,quality:.18,reliability:.16,brand:.1,age:.08,fit:.12,access:.06}},
      {id:'high_end',name:'High End',size:.17,growth:.06,price:145,weights:{price:.08,performance:.2,quality:.18,reliability:.16,brand:.12,age:.1,fit:.12,access:.04}},
      {id:'performance',name:'Performance',size:.19,growth:.045,price:125,weights:{price:.1,performance:.28,quality:.16,reliability:.2,brand:.06,age:.08,fit:.1,access:.02}},
      {id:'premium',name:'Premium',size:.16,growth:.03,price:185,weights:{price:.06,performance:.14,quality:.2,reliability:.18,brand:.2,age:.1,fit:.08,access:.04}}
    ],
    retail:[
      {id:'economy',name:'Ekonomik',size:.27,growth:.02,price:58,weights:{price:.3,performance:.08,quality:.14,reliability:.1,brand:.08,age:.08,fit:.1,access:.12}},
      {id:'mainstream',name:'Ana Akım',size:.29,growth:.03,price:92,weights:{price:.2,performance:.12,quality:.18,reliability:.16,brand:.1,age:.08,fit:.1,access:.06}},
      {id:'design',name:'Tasarım',size:.14,growth:.045,price:125,weights:{price:.1,performance:.1,quality:.18,reliability:.12,brand:.2,age:.08,fit:.16,access:.06}},
      {id:'performance',name:'Performans',size:.16,growth:.05,price:148,weights:{price:.1,performance:.24,quality:.16,reliability:.18,brand:.1,age:.08,fit:.1,access:.04}},
      {id:'premium',name:'Premium',size:.14,growth:.035,price:220,weights:{price:.05,performance:.14,quality:.2,reliability:.2,brand:.22,age:.08,fit:.07,access:.04}}
    ],
    food:[
      {id:'value',name:'Uygun Fiyat',size:.28,growth:.02,price:38,weights:{price:.32,performance:.04,quality:.16,reliability:.12,brand:.08,age:.06,fit:.12,access:.1}},
      {id:'family',name:'Aile',size:.25,growth:.025,price:55,weights:{price:.22,performance:.08,quality:.18,reliability:.18,brand:.1,age:.08,fit:.1,access:.06}},
      {id:'health',name:'Sağlıklı',size:.16,growth:.07,price:75,weights:{price:.1,performance:.06,quality:.24,reliability:.18,brand:.12,age:.08,fit:.16,access:.06}},
      {id:'convenience',name:'Kolaylık',size:.17,growth:.05,price:68,weights:{price:.14,performance:.18,quality:.14,reliability:.18,brand:.08,age:.06,fit:.1,access:.12}},
      {id:'gourmet',name:'Gurme',size:.14,growth:.045,price:115,weights:{price:.05,performance:.08,quality:.25,reliability:.18,brand:.2,age:.08,fit:.12,access:.04}}
    ]
  };
  const orgMeta={
    functional:{name:'Fonksiyonel',cost:.95,communication:.74,flexibility:.58,speed:.62,control:.72,copy:'Uzmanlık ve maliyet disiplini güçlü; departmanlar arası koordinasyon yavaştır.'},
    divisional:{name:'Bölümsel',cost:1.12,communication:.78,flexibility:.82,speed:.78,control:.56,copy:'Segment odaklı çeviklik sağlar; tekrar eden kaynak ve yönetim maliyeti yaratır.'},
    matrix:{name:'Matris',cost:1.18,communication:.64,flexibility:.9,speed:.52,control:.48,copy:'Kaynak paylaşımı ve inovasyon güçlü; çift raporlama çatışma yaratabilir.'},
    team:{name:'Takım Temelli',cost:.99,communication:.9,flexibility:.88,speed:.86,control:.42,copy:'Hızlı ve esnek; rol belirsizliği ile yönetişim riski daha yüksektir.'}
  };
  const executiveCatalog={
    CFO:{name:'Finans Direktörü',expertise:'sermaye ve risk',salary:42000,experience:7,leadership:72,loyalty:68,performance:74},
    COO:{name:'Operasyon Direktörü',expertise:'kapasite ve süreç',salary:38000,experience:6,leadership:76,loyalty:70,performance:73},
    CMO:{name:'Pazarlama Direktörü',expertise:'talep ve marka',salary:36000,experience:6,leadership:69,loyalty:74,performance:71},
    CTO:{name:'Teknoloji Direktörü',expertise:'ürün ve Ar-Ge',salary:46000,experience:8,leadership:67,loyalty:64,performance:79},
    CHRO:{name:'İK Direktörü',expertise:'kültür ve yetenek',salary:32000,experience:5,leadership:81,loyalty:78,performance:75},
    CSO:{name:'Strateji Direktörü',expertise:'pazar ve portföy',salary:40000,experience:7,leadership:73,loyalty:66,performance:76}
  };
  function ensureDeep(s){
    s=s||{};const industry=s.company?.industry||'saas',templates=segmentSets[industry]||segmentSets.saas;
    if(!s.segments||!Array.isArray(s.segments.market)||s.segments.market[0]?.id!==templates[0].id)s.segments={year:1,market:templates.map(x=>({...x,satisfaction:50,share:x.size*100,demand:0,unmet:0,criteriaShift:0})),lastAverage:50};
    s.organization=s.organization||{structure:'functional',centralization:65,delegation:35,span:5,culture:60,communication:60,changeReadiness:55,conflict:20,motivation:65,leadership:60,planning:55,organizing:55,leading:55,controlling:55};s.organization.departments=Array.isArray(s.organization.departments)&&s.organization.departments.length?s.organization.departments:['product','sales','operations','people'];
    s.executives=s.executives||{};Object.keys(executiveCatalog).forEach(role=>{if(!s.executives[role])s.executives[role]={role,...executiveCatalog[role],active:false,tenure:0,delegated:false}});
    s.finance=s.finance||{projects:[],cashFlows:[]};return s;
  }
  function scoreFactor(actual,target,range){return clamp(100-Math.abs(actual-target)/Math.max(range,1)*100)}
  function evaluateSegments(s,d={}){
    ensureDeep(s);const templates=segmentSets[s.company?.industry]||segmentSets.saas,price=Number(d.price??s.price??100),quality=Number(s.product?.quality??55),performance=Number(s.product?.performance??5)*10,brand=Number(s.brand??35),age=Number(s.product?.age??0),access=clamp((s.capacity||0)/Math.max(1,(d.expectedDemand||1000))*100),macro=s.economy?.demandIndex||1,year=Math.floor((s.period||0)/4)+1,customerBase=2200*macro*(1+Math.max(0,year-1)*.025),results=[];let weighted=0,totalDemand=0;
    templates.forEach((t,i)=>{const shift=(year-1)*.035*t.growth,idealQuality=45+i*10+shift*30,idealPerf=38+i*12+shift*22,priceFit=scoreFactor(price,t.price*(1+shift),t.price*.9),perfFit=scoreFactor(performance,idealPerf,75),qualityFit=scoreFactor(quality,idealQuality,70),reliability=clamp(78-quality*.12-age*4+(s.organization.motivation-60)*.12),reliabilityFit=clamp(reliability),brandFit=scoreFactor(brand,35+i*13+shift*30,80),ageFit=clamp(100-age*(8+i*1.5)),fit=clamp((perfFit+qualityFit+reliabilityFit)/3),criteria=t.weights.price*priceFit+t.weights.performance*perfFit+t.weights.quality*qualityFit+t.weights.reliability*reliabilityFit+t.weights.brand*brandFit+t.weights.age*ageFit+t.weights.fit*fit+t.weights.access*access,expectation=clamp(criteria+shift*18),demand=Math.max(0,Math.round(customerBase*t.size*(1+t.growth*Math.max(0,year-1)))),competitorPressure=clamp((s.rivals||[]).reduce((sum,r)=>sum+(r.quality||60)/Math.max(50,r.price||100),0)*7),satisfaction=clamp(expectation-competitorPressure*.12),share=clamp(satisfaction*(1+((s.marketShare||4)-4)/120)),unmet=Math.max(0,Math.round(demand*(1-satisfaction/100)*.45));results.push({...t,satisfaction:Math.round(satisfaction),share:Math.round(share*100)/100,demand,unmet,criteriaShift:Math.round(shift*1000)/10});weighted+=satisfaction*t.size;totalDemand+=demand});s.segments={year,market:results,lastAverage:Math.round(weighted)};return {results,weighted:Math.round(weighted),totalDemand};
  }
  function orgEffects(s){ensureDeep(s);const o=s.organization,m=orgMeta[o.structure]||orgMeta.functional,active=Object.values(s.executives).filter(x=>x.active),cto=active.find(x=>x.role==='CTO'),coo=active.find(x=>x.role==='COO'),cfo=active.find(x=>x.role==='CFO'),cmo=active.find(x=>x.role==='CMO'),deptPenalty=Math.max(0,(o.departments?.length||4)-4)*3,eff=clamp(55*m.communication+o.delegation*.22+o.motivation*.18+o.culture*.15+(coo?.performance||0)*.12-deptPenalty,10,100),marketing=clamp(55*m.flexibility+o.delegation*.25+(cmo?.performance||0)*.15-deptPenalty*.5,10,100),innovation=clamp(45+o.changeReadiness*.35+(cto?.performance||0)*.22,10,100),risk=clamp(55*m.control+o.centralization*.2+(cfo?.performance||0)*.2-o.conflict*.2-deptPenalty,10,100);return {eff,marketing,innovation,risk,cost:m.cost,meta:m,departments:o.departments.length}}
  function irr(cashFlows){const npvAt=rate=>cashFlows.reduce((sum,cf,t)=>sum+cf/Math.pow(1+rate,t),0);let low=-.9999,high=10,nLow=npvAt(low),nHigh=npvAt(high);if(nLow*nHigh>0)return nLow>0?high:low;for(let i=0;i<80;i++){const mid=(low+high)/2,nMid=npvAt(mid);if(Math.abs(nMid)<.0001)return mid;if(nLow*nMid<=0){high=mid;nHigh=nMid}else{low=mid;nLow=nMid}}return (low+high)/2}
  function financeMetrics(s,p={}){ensureDeep(s);const active=Object.values(s.executives).filter(x=>x.active),debt=s.debtPrincipal||0,equity=Math.max(1,s.equity||s.cash||1),tax=.25,riskFree=.24,beta=1.05,marketPremium=.09,costEquity=riskFree+beta*marketPremium,afterTaxDebt=(s.economy?.interest||.24)*(1-tax),wacc=(equity/(equity+debt))*costEquity+(debt/(equity+debt))*afterTaxDebt,investment=Math.max(0,Number(p.investment??250000)),annual=Math.max(0,Number(p.annualCashflow??90000)),horizon=Math.max(1,Math.min(15,Math.floor(Number(p.horizon??5)))),discount=Math.max(.01,Number(p.discount??wacc));const flows=[-investment,...Array.from({length:horizon},()=>annual)],npv=flows.reduce((sum,cf,t)=>sum+cf/Math.pow(1+discount,t),0),rate=irr(flows),payback=annual>0?investment/annual:Infinity,last=s.history?.[0]?.profit||{},revenue=last.revenue||0,variable=last.cogs||0,contribution=Math.max(0,revenue-variable),fixed=Math.max(0,(last.ebitda||0)<0?Math.abs(last.ebitda):0),breakEven=contribution>0?fixed/(contribution/Math.max(1,revenue)):Infinity,workingCapital=(s.inventory||0)+(s.receivables||0)-(s.payables||0);return {npv,irr:rate,payback,wacc,contributionMargin:revenue?contribution/revenue:0,breakEvenRevenue:breakEven,workingCapital,costOfEquity:costEquity,afterTaxDebt,executivePayroll:active.reduce((sum,x)=>sum+x.salary,0)/4};}
  function recalcState(s,d){ensureDeep(s);const before=evaluateSegments(s,d),o=orgEffects(s),adj={...d,productionUnits:Number(d.productionUnits??s.capacity*.7)*(0.72+o.eff/360),marketing:Number(d.marketing??30000)*(0.72+o.marketing/300),rnd:Number(d.rnd??15000)*(0.72+o.innovation/300)};return {before,o,adj};}
  const baseAdvance=E.advanceQuarter;
  E.advanceQuarter=function(input,decisions={},random){const cloned=E.normalizeState(JSON.parse(JSON.stringify(input||{}))),prep=recalcState(cloned,decisions),result=baseAdvance(cloned,prep.adj,random),s=ensureDeep(result.state),r=result.report;const seg=evaluateSegments(s,decisions),o=orgEffects(s),execPayroll=Object.values(s.executives).filter(x=>x.active).reduce((sum,x)=>sum+x.salary,0)/4,paidExec=Math.min(s.cash,execPayroll);if(paidExec){s.cash-=paidExec;s.equity-=paidExec;s.retainedEarnings-=paidExec;r.profit.ebitda-=paidExec;r.profit.netIncome-=paidExec;r.cashflow.operating-=paidExec;r.balance.cash=s.cash;r.balance.equity=s.equity}const fm=financeMetrics(s,{investment:decisions.investment,annualCashflow:decisions.annualCashflow,horizon:decisions.horizon,discount:decisions.discount});r.segments=seg.results.map(x=>({id:x.id,name:x.name,satisfaction:x.satisfaction,demand:x.demand,share:x.share,unmet:x.unmet}));r.market.segmentAverageSatisfaction=seg.weighted;r.market.unmetDemand=r.segments.reduce((sum,x)=>sum+x.unmet,0);r.organization={structure:s.organization.structure,efficiency:Math.round(o.eff),marketingPower:Math.round(o.marketing),innovationPower:Math.round(o.innovation),riskControl:Math.round(o.risk),costMultiplier:o.cost};r.finance=fm;const assets=s.cash+s.inventory+s.receivables+s.fixedAssets-s.accumulatedDepreciation,liabilities=s.payables+s.debtPrincipal;r.audit.balanceError=Math.round((assets-liabilities-s.equity)*100)/100;r.audit.violations=r.audit.violations||[];if(r.audit.balanceError!==0&&!r.audit.violations.includes('balance_sheet_mismatch'))r.audit.violations.push('balance_sheet_mismatch');s.audit=r.audit;s.history[0]=r;return result;};
  window.deepStrategyEnsure=ensureDeep;window.deepStrategyFinance=financeMetrics;window.deepStrategySegments=evaluateSegments;window.deepStrategyOrg=orgEffects;
})();
