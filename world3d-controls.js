(()=>{
  const pending={speed:null,pause:false};
  window.__worldSetSpeed=(speed)=>{
    if(typeof window.__worldApplySpeed==='function')window.__worldApplySpeed(speed);
    else pending.speed=speed;
  };
  window.__worldTogglePause=()=>{
    if(typeof window.__worldApplyPause==='function')window.__worldApplyPause();
    else pending.pause=!pending.pause;
  };
  window.__worldConsumePending=()=>{const value={...pending};pending.speed=null;pending.pause=false;return value;};
})();
