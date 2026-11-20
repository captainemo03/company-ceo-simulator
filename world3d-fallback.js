(()=>{
  const canvas=document.getElementById('worldCanvas');
  if(!canvas)return;
  let speed=1, paused=false, camera='free', frame=0;
  const agents=Array.from({length:10},(_,i)=>({x:0.2+(i%5)*0.14,y:0.58+Math.floor(i/5)*0.1,phase:i*0.7}));
  const draw=()=>{
    if(canvas.dataset.worldReady==='1')return;
    const ctx=canvas.getContext('2d'); if(!ctx)return;
    const w=canvas.width=canvas.clientWidth||1000,h=360;
    const bg=ctx.createLinearGradient(0,0,w,h);bg.addColorStop(0,'#172957');bg.addColorStop(1,'#10162f');ctx.fillStyle=bg;ctx.fillRect(0,0,w,h);
    ctx.fillStyle='#263e71';ctx.beginPath();ctx.moveTo(w*.08,h*.82);ctx.lineTo(w*.38,h*.48);ctx.lineTo(w*.9,h*.62);ctx.lineTo(w*.72,h*.9);ctx.closePath();ctx.fill();
    ctx.fillStyle='#344f87';ctx.fillRect(w*.22,h*.35,w*.18,h*.3);ctx.fillRect(w*.48,h*.28,w*.22,h*.37);
    agents.forEach((a,i)=>{const drift=paused?0:Math.sin(frame*.012*speed+a.phase)*.028;const x=(a.x+drift)*w,y=a.y*h;ctx.fillStyle=['#68d3c5','#f3b562','#9ca9ff','#ed8eaa','#7dd0f2'][i%5];ctx.beginPath();ctx.arc(x,y-11,7,0,Math.PI*2);ctx.fill();ctx.fillRect(x-6,y-4,12,18);});
    ctx.fillStyle='#cdd8ff';ctx.font='600 14px Manrope, sans-serif';ctx.fillText('WebGL yedeği · otonom gözlem modu',24,32);ctx.fillStyle='#9fb3df';ctx.font='12px Manrope, sans-serif';ctx.fillText(`Kamera: ${camera} · Zaman: ${paused?'duraklatıldı':speed+'x'}`,24,54);
    if(!paused)requestAnimationFrame(()=>{frame++;draw()});
  };
  const fallback=()=>{if(canvas.dataset.worldReady==='1')return;draw();
    document.querySelectorAll('[data-world-speed]').forEach(btn=>btn.addEventListener('click',()=>{speed=Number(btn.dataset.worldSpeed)||1;paused=false;draw()}));
    document.getElementById('worldPause')?.addEventListener('click',()=>{paused=!paused;draw()});
    document.querySelectorAll('[data-world-camera]').forEach(btn=>btn.addEventListener('click',()=>{camera=btn.dataset.worldCamera||'free';draw()}));
    document.getElementById('worldCharacterSelect')?.addEventListener('change',()=>draw());
  };
  window.addEventListener('error',event=>{if(String(event.filename||'').includes('world3d'))fallback()});
  window.addEventListener('unhandledrejection',event=>{if(String(event.reason||'').includes('three'))fallback()});
  setTimeout(()=>{if(canvas.dataset.worldReady!=='1'&&!window.__world3dLoaded)fallback()},1800);
})();
