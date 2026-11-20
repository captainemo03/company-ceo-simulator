(function(){
  'use strict';
  const E=window.BusinessEmpireEngine;
  if(!E)return;
  let mounted=false;
  const currentGame=()=>{try{return game||null}catch{return null}};
  const money=n=>'₺'+Math.round(Number(n)||0).toLocaleString('tr-TR');
  const pct=n=>`${Math.round((Number(n)||0)*100)}%`;
  const esc=v=>String(v??'').replace(/[&<>"']/g,c=>({'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;',"'":'&#39;'}[c]));
  const markup='<section class="panel audit-card"><div class="panel-heading"><div><span class="eyebrow">ÇİFT KAYITLI FİNANS DENETİMİ</span><h3>Defter-i kebir ve oranlar</h3></div><b id="engineAuditStatus" class="tag neutral">BEKLENİYOR</b></div><div class="audit-grid"><span><b>Net kâr</b><strong id="auditNetIncome">—</strong></span><span><b>Brüt marj</b><strong id="auditGrossMargin">—</strong></span><span><b>EBITDA marjı</b><strong id="auditEbitdaMargin">—</strong></span><span><b>Cari oran</b><strong id="auditCurrentRatio">—</strong></span><span><b>ROA / ROE</b><strong id="auditRoaRoe">—</strong></span><span><b>Bilanço farkı</b><strong id="auditBalanceError">—</strong></span></div><p id="auditCopy">Motor, her turu bağımsız bir bilanço ve nakit akışı denetiminden geçirir.</p></section>';
  function ensureState(){
    const g=currentGame();
    if(!g)return null;
    if(!g.engineState){
      g.engineState=E.createState({name:g.name,industry:g.industry,cash:Math.max(50000,g.cash||0),debt:g.debt||0,team:Math.max(2,g.team||4),quality:Math.max(35,g.quality||55),capacity:Math.max(60,Math.round((g.customers||100)/5)),price:100});
    }
    return g.engineState;
  }
  function decisions(){
    const g=currentGame()||{},pricing=document.getElementById('pricing')?.value||'value';
    return {price:{growth:78,value:100,premium:132}[pricing]||100,marketing:Number(document.getElementById('marketing')?.value)||25000,salesBudget:Math.max(5000,Number(document.getElementById('marketing')?.value||25000)*.35),productionUnits:Math.max(30,Math.round((g.customers||100)*.25+((g.team||4)*18))),rawMaterialOrder:Math.max(50,Math.round((g.customers||100)*.3+((g.team||4)*22))),rnd:Number(document.getElementById('product')?.value)||15000,training:(g.v3?.classroom?.mode==='teacher'?12000:5000),debtPayment:Math.min(g.debt||0,10000),acquisitionTarget:g.pendingAcquisition||null,marketResearch:g.marketResearchBudget||0};
  }
  function mount(){
    if(mounted)return;
    const anchor=document.querySelector('.metrics-v3')||document.querySelector('.scenario-card');
    if(!anchor)return;
    anchor.insertAdjacentHTML('afterend',markup);mounted=true;
  }
  function paint(){
    const g=currentGame();
    if(!g)return;
    mount();
    const report=g.engineAudit;
    if(!report)return;
    const p=report.profit||{},b=report.balance||{},a=report.audit||{},assets=(b.cash||0)+(b.inventory||0)+(b.receivables||0)+(b.fixedAssets||0),liabilities=b.liabilities||0;
    const currentRatio=(b.cash+b.inventory+b.receivables)/Math.max(1,liabilities),quickRatio=(b.cash+b.receivables)/Math.max(1,liabilities),grossMargin=p.grossProfit/Math.max(1,p.revenue),ebitdaMargin=p.ebitda/Math.max(1,p.revenue),roa=p.netIncome/Math.max(1,assets),roe=p.netIncome/Math.max(1,Math.abs(b.equity));
    const put=(id,val)=>{const n=document.getElementById(id);if(n)n.textContent=val};
    put('auditNetIncome',money(p.netIncome));put('auditGrossMargin',pct(grossMargin));put('auditEbitdaMargin',pct(ebitdaMargin));put('auditCurrentRatio',`${currentRatio.toFixed(2)} / ${quickRatio.toFixed(2)}`);put('auditRoaRoe',`${pct(roa)} / ${pct(roe)}`);put('auditBalanceError',money(a.balanceError));
    const status=document.getElementById('engineAuditStatus');
    if(status){const ok=!a.violations?.length;status.textContent=ok?'DENGELİ':'İNCELEME GEREKİYOR';status.className='tag '+(ok?'up':'down')}
    put('auditCopy',a.violations?.length?`Denetim uyarısı: ${a.violations.map(esc).join(' · ')}`:`Bağımsız motor dengeli: ${report.period}. dönem · nakit ${money(b.cash)} · yükümlülük ${money(liabilities)}.`);
    if(typeof window.deepStrategyPaint==='function')window.deepStrategyPaint();
  }
  const oldRender=window.render;
  if(typeof oldRender==='function')window.render=function(){oldRender();paint()};
  const oldAdvance=window.advance;
  if(typeof oldAdvance==='function')window.advance=function(){
    const g=currentGame();
    if(g&&!g.ended){const s=ensureState();const r=E.advanceQuarter(s,decisions());g.engineState=r.state;g.engineAudit=r.report;g.pendingAcquisition=null;g.marketResearchBudget=0}
    return oldAdvance.apply(this,arguments);
  };
  const advanceButton=document.getElementById('advanceBtn');
  if(advanceButton&&typeof window.advance==='function')advanceButton.onclick=window.advance;
  window.engineAuditPaint=paint;
})();
