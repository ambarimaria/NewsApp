document.addEventListener('DOMContentLoaded', () => {

  // Navbar shadow on scroll
  const nav = document.getElementById('mainNavbar');
  if (nav) window.addEventListener('scroll', () => nav.classList.toggle('scrolled', scrollY > 8), { passive: true });

  // Reading progress bar (article detail only)
  if (document.querySelector('.article-title')) {
    const bar = document.createElement('div');
    bar.style.cssText = 'position:fixed;top:0;left:0;height:3px;width:0%;background:linear-gradient(90deg,var(--accent),var(--accent-2));z-index:9999;transition:width .1s linear;pointer-events:none;border-radius:0 2px 2px 0;';
    document.body.prepend(bar);
    window.addEventListener('scroll', () => {
      const t = document.body.scrollHeight - window.innerHeight;
      bar.style.width = t > 0 ? `${(scrollY / t) * 100}%` : '0%';
    }, { passive: true });
  }

  // Image error fallback
  document.querySelectorAll('img[loading="lazy"]').forEach(img => {
    img.addEventListener('error', function () {
      const wrap = this.closest('.news-card-img-wrap, .sidebar-story-img-wrap');
      if (wrap) wrap.innerHTML = '<div class="news-card-img-placeholder"><i class="fas fa-newspaper"></i></div>';
      else this.style.display = 'none';
    });
  });

  // Scroll active category chip into view
  const active = document.querySelector('.category-chip.active');
  if (active) active.scrollIntoView({ behavior: 'smooth', block: 'nearest', inline: 'center' });

  // Prevent empty search submit
  document.querySelectorAll('form[action*="Search"]').forEach(f => {
    f.addEventListener('submit', e => {
      const q = f.querySelector('[name="q"]');
      if (q && !q.value.trim()) {
        e.preventDefault();
        q.focus();
        q.style.borderColor = 'var(--accent)';
        q.style.boxShadow = '0 0 0 3px var(--accent-glow)';
        setTimeout(() => { q.style.borderColor = ''; q.style.boxShadow = ''; }, 1800);
      }
    });
  });

});

// Toast notification (used by share button)
function showToast(msg) {
  const t = document.createElement('div');
  t.textContent = msg;
  t.style.cssText = 'position:fixed;bottom:1.5rem;right:1.5rem;background:var(--accent);color:#fff;padding:.55rem 1.1rem;border-radius:7px;font-size:.8rem;font-weight:700;z-index:9999;box-shadow:0 8px 28px var(--accent-glow);animation:fadeUp .3s var(--ease) both';
  document.body.appendChild(t);
  setTimeout(() => t.remove(), 2800);
}
