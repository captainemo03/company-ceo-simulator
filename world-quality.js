(()=>{
  const presets={
    low:{pixelRatio:.85,shadows:false,shadowMap:512,viewDistance:28,npcDensity:.55,effects:false,fps:30},
    medium:{pixelRatio:1,shadows:true,shadowMap:1024,viewDistance:38,npcDensity:.8,effects:true,fps:45},
    high:{pixelRatio:1.35,shadows:true,shadowMap:1536,viewDistance:52,npcDensity:1,effects:true,fps:60},
    ultra:{pixelRatio:1.7,shadows:true,shadowMap:2048,viewDistance:72,npcDensity:1,effects:true,fps:60}
  };
  const key='sirket-ceo-world-settings';
  function get(){try{return JSON.parse(localStorage.getItem(key))||{profile:'medium',weather:'sunny',language:'tr-TR',audio:{master:.35,ambient:.3,voices:.8,fx:.45,muted:true}}}catch{return {profile:'medium',weather:'sunny',language:'tr-TR',audio:{master:.35,ambient:.3,voices:.8,fx:.45,muted:true}}}}
  function save(s){try{localStorage.setItem(key,JSON.stringify(s))}catch{}}
  function preset(){const s=get();return presets[s.profile]||presets.medium}
  function apply(renderer,scene,sun){const p=preset();renderer.setPixelRatio(Math.min(window.devicePixelRatio||1,p.pixelRatio));renderer.shadowMap.enabled=p.shadows;if(sun){sun.castShadow=p.shadows;sun.shadow.mapSize.set(p.shadowMap,p.shadowMap)}if(scene?.fog)scene.fog.far=p.viewDistance;return p}
  let audio=null;
  function audioStart(){if(audio)return audio;const C=window.AudioContext||window.webkitAudioContext;if(!C)return null;const ctx=new C(),master=ctx.createGain(),ambient=ctx.createGain(),osc=ctx.createOscillator();master.gain.value=get().audio?.master??.35;ambient.gain.value=(get().audio?.ambient??.3)*.12;osc.type='sine';osc.frequency.value=92;osc.connect(ambient).connect(master).connect(ctx.destination);osc.start();audio={ctx,master,ambient,osc};ctx.resume?.();return audio}
  function audioUpdate(mode='office',weather='sunny'){const a=audio||audioStart();if(!a)return;const s=get(),factor=weather==='storm'?1.35:weather==='rain'?1.15:weather==='fog'?.8:1;a.ambient.gain.value=(s.audio?.ambient??.3)*.12*factor;a.osc.frequency.value=mode==='operations'?128:mode==='city'?72:92}
  function bind(){const s=get(),profile=document.getElementById('worldGraphicsProfile'),weather=document.getElementById('worldWeatherSelect'),lang=document.getElementById('worldLanguage');if(profile){profile.value=s.profile;profile.onchange=()=>{const n=get();n.profile=profile.value;save(n);location.reload()}}if(weather){weather.value=s.weather;weather.onchange=()=>{const n=get();n.weather=weather.value;save(n);window.dispatchEvent(new CustomEvent('world-weather',{detail:weather.value}))}}if(lang){lang.value=s.language;lang.onchange=()=>{const n=get();n.language=lang.value;save(n)}}document.getElementById('worldAudioStart')?.addEventListener('click',()=>{const a=audioStart();if(a){const n=get();n.audio.muted=false;save(n);document.getElementById('worldAudioStart').textContent='Ortam sesi: Açık'}});['master','ambient','voices','fx'].forEach(k=>{const el=document.getElementById('worldAudio'+k[0].toUpperCase()+k.slice(1));if(el)el.value=s.audio?.[k]??.3;el?.addEventListener('input',()=>{const n=get();n.audio=n.audio||{};n.audio[k]=Number(el.value);save(n);if(k==='master'&&audio)audio.master.gain.value=n.audio[k]})})}
  window.WorldQuality={presets,get,preset,save,apply,bind};
  window.WorldAudio={start:audioStart,update:audioUpdate};
  bind();
})();
