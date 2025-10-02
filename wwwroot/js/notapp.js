// Hamburger menü
(function(){
  const btn = document.getElementById('menuToggle');
  const menu = document.getElementById('slideMenu');
  if(btn && menu){
    btn.addEventListener('click', () => menu.classList.toggle('open'));
    document.addEventListener('click', (e)=>{
      if(!menu.contains(e.target) && !btn.contains(e.target)) menu.classList.remove('open');
    });
  }
})();

// Fakülte -> Bölüm bağımlı seçim
const FACULTIES = {
  "Teknoloji Fakültesi": [
    "Yazılım Mühendisliği","Bilgisayar Mühendisliği","Elektrik-Elektronik Mühendisliği"
  ],
  "Mühendislik Fakültesi": [
    "Bilgisayar Mühendisliği","Makine Mühendisliği","İnşaat Mühendisliği"
  ],
  "İktisadi ve İdari Bilimler Fakültesi": [
    "İşletme","İktisat","Siyaset Bilimi ve Kamu Yönetimi"
  ],
  "Fen Fakültesi": ["Matematik","Fizik","Kimya"],
  "Sağlık Bilimleri Fakültesi": ["Hemşirelik","Fizyoterapi ve Rehabilitasyon"]
  // ihtiyaca göre genişlet
};

function wireFacultyDepartment(facSel, depSel){
  if(!facSel || !depSel) return;
  // Fakülte seçeneklerini doldur (eğer boşsa)
  if(facSel.options.length <= 1){
    Object.keys(FACULTIES).forEach(f=>{
      const opt = document.createElement('option');
      opt.value = f; opt.textContent = f;
      facSel.appendChild(opt);
    });
  }
  const fillDeps = () => {
    depSel.innerHTML = "";
    const def = document.createElement('option');
    def.value=""; def.textContent="Bölüm Seç";
    depSel.appendChild(def);
    const list = FACULTIES[facSel.value] || [];
    list.forEach(d=>{
      const o=document.createElement('option'); o.value=d; o.textContent=d;
      depSel.appendChild(o);
    });
    depSel.disabled = list.length === 0;
  };
  facSel.addEventListener('change', fillDeps);
  fillDeps();
}

// Create sayfasında bağla
document.addEventListener('DOMContentLoaded', ()=>{
  const fac = document.querySelector('[data-faculty]');
  const dep = document.querySelector('[data-department]');
  if(fac && dep) wireFacultyDepartment(fac, dep);
});
