document.addEventListener('DOMContentLoaded', () => {
  const reducedMotion = window.matchMedia('(prefers-reduced-motion: reduce)').matches;
  const refreshIcons = () => window.lucide?.createIcons();
  refreshIcons();

  const drawer = document.querySelector('.portfolio-sidebar');
  const drawerButton = document.querySelector('[data-drawer-toggle]');
  const drawerBackdrop = document.querySelector('[data-drawer-backdrop]');
  const setDrawer = open => {
    drawer?.classList.toggle('open', open);
    drawerBackdrop?.classList.toggle('open', open);
    drawerButton?.setAttribute('aria-expanded', String(open));
    document.body.classList.toggle('drawer-open', open);
    const icon = drawerButton?.querySelector('svg');
    if (icon) icon.outerHTML = `<i data-lucide="${open ? 'x' : 'menu'}"></i>`;
    refreshIcons();
  };
  drawerButton?.addEventListener('click', () => setDrawer(!drawer?.classList.contains('open')));
  drawerBackdrop?.addEventListener('click', () => setDrawer(false));
  drawer?.querySelectorAll('a[href^="#"]').forEach(link => link.addEventListener('click', () => setDrawer(false)));
  document.addEventListener('keydown', event => { if (event.key === 'Escape') setDrawer(false); });

  const revealItems = [...document.querySelectorAll('[data-reveal], .reveal')];
  revealItems.forEach((item, index) => item.style.setProperty('--reveal-delay', `${Math.min(index % 6, 5) * 65}ms`));
  if (reducedMotion) revealItems.forEach(item => item.classList.add('visible'));
  else {
    const revealObserver = new IntersectionObserver(entries => entries.forEach(entry => {
      if (!entry.isIntersecting) return;
      entry.target.classList.add('visible');
      revealObserver.unobserve(entry.target);
    }), { threshold: 0.12, rootMargin: '0px 0px -8% 0px' });
    revealItems.forEach(item => revealObserver.observe(item));
  }

  const navLinks = [...document.querySelectorAll('.portfolio-nav a[href^="#"]')];
  if (navLinks.length) {
    const sections = navLinks.map(link => document.querySelector(link.hash)).filter(Boolean);
    const activate = id => navLinks.forEach(link => {
      const active = link.hash === `#${id}`;
      link.classList.toggle('active', active);
      if (active) link.setAttribute('aria-current', 'location'); else link.removeAttribute('aria-current');
    });
    const sectionObserver = new IntersectionObserver(entries => {
      const visible = entries.filter(entry => entry.isIntersecting).sort((a, b) => b.intersectionRatio - a.intersectionRatio)[0];
      if (visible) activate(visible.target.id);
    }, { rootMargin: '-22% 0px -58% 0px', threshold: [0.08, 0.3, 0.6] });
    sections.forEach(section => sectionObserver.observe(section));
  }

  const progress = document.querySelector('[data-scroll-progress]');
  const toTop = document.querySelector('[data-scroll-top]');
  const updateScroll = () => {
    const available = document.documentElement.scrollHeight - innerHeight;
    const ratio = available > 0 ? Math.min(scrollY / available, 1) : 0;
    if (progress) progress.style.transform = `scaleX(${ratio})`;
    toTop?.classList.toggle('visible', scrollY > 650);
  };
  addEventListener('scroll', updateScroll, { passive: true });
  updateScroll();
  toTop?.addEventListener('click', () => scrollTo({ top: 0, behavior: reducedMotion ? 'auto' : 'smooth' }));

  const projectButtons = [...document.querySelectorAll('[data-project-filter]')];
  const projectCards = [...document.querySelectorAll('[data-project-category]')];
  projectButtons.forEach(button => button.addEventListener('click', () => {
    const category = button.dataset.projectFilter;
    projectButtons.forEach(item => {
      const active = item === button;
      item.classList.toggle('active', active);
      item.setAttribute('aria-pressed', String(active));
    });
    projectCards.forEach(card => {
      const match = category === 'all' || card.dataset.projectCategory === category;
      card.hidden = !match;
      if (match && !reducedMotion) card.animate([{ opacity: 0, transform: 'translateY(12px)' }, { opacity: 1, transform: 'translateY(0)' }], { duration: 280, easing: 'ease-out' });
    });
  }));

  document.querySelectorAll('[data-current-toggle]').forEach(toggle => {
    const endDate = toggle.closest('form')?.querySelector('[name="EndDate"]');
    const sync = () => { if (endDate) { endDate.disabled = toggle.checked; if (toggle.checked) endDate.value = ''; } };
    toggle.addEventListener('change', sync); sync();
  });

  document.querySelectorAll('[data-certificate-no-expiry]').forEach(toggle => {
    const expiration = toggle.closest('form')?.querySelector('[data-certificate-expiration]');
    const sync = () => {
      if (!expiration) return;
      expiration.disabled = toggle.checked;
      if (toggle.checked) expiration.value = '';
    };
    toggle.addEventListener('change', sync);
    sync();
  });

  document.querySelectorAll('[data-preview-source]').forEach(source => {
    const preview = source.closest('form')?.querySelector('[data-content-preview]');
    if (!preview) return;
    const render = () => {
      preview.innerHTML = source.value.trim() || '<p class="preview-placeholder">Your live content preview will appear here.</p>';
      preview.classList.toggle('has-content', Boolean(source.value.trim()));
    };
    source.addEventListener('input', render); render();
  });

  document.querySelectorAll('[data-editor-wrap]').forEach(button => button.addEventListener('click', () => {
    const textarea = button.closest('.markdown-field')?.querySelector('textarea');
    if (!textarea) return;
    const [before, after = ''] = button.dataset.editorWrap.split('|');
    const start = textarea.selectionStart;
    const end = textarea.selectionEnd;
    const selection = textarea.value.slice(start, end) || 'text';
    textarea.setRangeText(`${before}${selection}${after}`, start, end, 'select');
    textarea.dispatchEvent(new Event('input', { bubbles: true }));
    textarea.focus();
  }));

  document.querySelectorAll('[data-image-input]').forEach(input => input.addEventListener('change', () => {
    const label = input.closest('.form-field')?.querySelector('small');
    if (label && input.files?.length) label.textContent = input.files.length === 1 ? input.files[0].name : `${input.files.length} files selected`;
  }));

  const status = document.querySelector('[data-admin-status]');
  const showStatus = (message, error = false) => {
    if (!status) return;
    status.textContent = message;
    status.classList.toggle('error', error);
    clearTimeout(showStatus.timer);
    showStatus.timer = setTimeout(() => { status.textContent = ''; status.classList.remove('error'); }, 3000);
  };
  document.querySelectorAll('[data-reorder-table]').forEach(wrapper => {
    const body = wrapper.querySelector('tbody');
    let dragging = null;
    const rows = () => [...body.querySelectorAll('tr[data-id]')];
    const save = async () => {
      const payload = rows().map((row, index) => ({ id: row.dataset.id, displayOrder: index + 1 }));
      rows().forEach((row, index) => { const order = row.querySelector('[data-order]'); if (order) order.textContent = String(index + 1); });
      const token = wrapper.querySelector('input[name="__RequestVerificationToken"]')?.value || document.querySelector('input[name="__RequestVerificationToken"]')?.value;
      try {
        const response = await fetch(`/admin/reorder/${wrapper.dataset.type}`, { method: 'POST', headers: { 'Content-Type': 'application/json', 'X-CSRF-TOKEN': token || '' }, body: JSON.stringify(payload) });
        if (!response.ok) throw new Error();
        showStatus('Order saved');
      } catch { showStatus('Could not save order', true); }
    };
    body.addEventListener('dragstart', event => { dragging = event.target.closest('tr[data-id]'); dragging?.classList.add('dragging'); event.dataTransfer.effectAllowed = 'move'; });
    body.addEventListener('dragover', event => {
      event.preventDefault();
      const target = event.target.closest('tr[data-id]');
      if (!dragging || !target || dragging === target) return;
      const rect = target.getBoundingClientRect();
      body.insertBefore(dragging, event.clientY < rect.top + rect.height / 2 ? target : target.nextSibling);
    });
    body.addEventListener('dragend', () => { dragging?.classList.remove('dragging'); dragging = null; save(); });
    body.addEventListener('keydown', event => {
      const handle = event.target.closest('.drag-handle');
      if (!handle || !['ArrowUp', 'ArrowDown'].includes(event.key)) return;
      event.preventDefault();
      const row = handle.closest('tr');
      const sibling = event.key === 'ArrowUp' ? row.previousElementSibling : row.nextElementSibling;
      if (!sibling) return;
      if (event.key === 'ArrowUp') body.insertBefore(row, sibling); else body.insertBefore(sibling, row);
      handle.focus(); save();
    });
  });

  document.querySelectorAll('[data-tech-icon-image]').forEach(image => image.addEventListener('error', () => image.closest('.technology-icon')?.classList.add('image-failed')));
  document.querySelectorAll('[data-image-fallback]').forEach(image => image.addEventListener('error', () => image.classList.add('image-failed')));

  const adminSidebar = document.querySelector('[data-admin-sidebar]');
  const adminBackdrop = document.querySelector('.cms-sidebar-backdrop');
  const setAdminSidebar = open => {
    adminSidebar?.classList.toggle('open', open);
    adminBackdrop?.classList.toggle('open', open);
    document.body.classList.toggle('drawer-open', open);
  };
  document.querySelector('[data-admin-sidebar-open]')?.addEventListener('click', () => setAdminSidebar(true));
  document.querySelectorAll('[data-admin-sidebar-close]').forEach(button => button.addEventListener('click', () => setAdminSidebar(false)));
  document.querySelectorAll('[data-dismiss-toast]').forEach(button => button.addEventListener('click', () => button.closest('.cms-toast')?.classList.add('dismissed')));
  const successToast = document.querySelector('.cms-toast.success');
  if (successToast) setTimeout(() => successToast.classList.add('dismissed'), 4800);

  document.querySelectorAll('[data-category-icon]').forEach(button => button.addEventListener('click', () => {
    const group = button.closest('.category-icon-options');
    group?.querySelectorAll('button').forEach(item => item.classList.toggle('selected', item === button));
    const input = button.closest('fieldset')?.querySelector('[data-category-icon-value]');
    if (input) input.value = button.dataset.categoryIcon;
  }));
  document.querySelectorAll('[data-color-preset]').forEach(button => button.addEventListener('click', () => {
    const group = button.closest('.color-preset-options');
    group?.querySelectorAll('button').forEach(item => item.classList.toggle('selected', item === button));
    const input = button.closest('fieldset')?.querySelector('[data-color-value]');
    if (input) input.value = button.dataset.colorPreset;
  }));

  const token = () => document.querySelector('input[name="__RequestVerificationToken"]')?.value || '';
  document.querySelectorAll('[data-reorder-list]').forEach(wrapper => {
    let dragging = null;
    const directItems = () => [...wrapper.children].filter(item => item.matches('[data-id]'));
    const directItem = target => {
      const item = target.closest('[data-id]');
      return item?.parentElement === wrapper ? item : null;
    };
    const save = async () => {
      const payload = directItems().map((item, index) => ({ id: item.dataset.id, displayOrder: index + 1 }));
      if (!payload.length) return;
      try {
        const response = await fetch(`/admin/reorder/${wrapper.dataset.type}`, { method: 'POST', headers: { 'Content-Type': 'application/json', 'X-CSRF-TOKEN': token() }, body: JSON.stringify(payload) });
        if (!response.ok) throw new Error();
        showStatus('Order updated.');
      } catch { showStatus('Could not update the order.', true); }
    };
    wrapper.addEventListener('dragstart', event => {
      const item = directItem(event.target);
      if (!item) return;
      dragging = item;
      item.classList.add('dragging');
      event.dataTransfer.effectAllowed = 'move';
    });
    wrapper.addEventListener('dragover', event => {
      if (!dragging) return;
      event.preventDefault();
      const target = directItem(event.target);
      if (!target || target === dragging) return;
      const rect = target.getBoundingClientRect();
      wrapper.insertBefore(dragging, event.clientY < rect.top + rect.height / 2 ? target : target.nextSibling);
    });
    wrapper.addEventListener('dragend', () => {
      if (!dragging) return;
      dragging.classList.remove('dragging');
      dragging = null;
      save();
    });
    wrapper.addEventListener('keydown', event => {
      const handle = event.target.closest('.card-drag-handle');
      if (!handle || !['ArrowUp', 'ArrowDown'].includes(event.key)) return;
      const item = directItem(handle);
      if (!item) return;
      const sibling = event.key === 'ArrowUp' ? item.previousElementSibling : item.nextElementSibling;
      if (!sibling?.matches('[data-id]')) return;
      event.preventDefault();
      if (event.key === 'ArrowUp') wrapper.insertBefore(item, sibling); else wrapper.insertBefore(sibling, item);
      handle.focus();
      save();
    });
  });

  const confirmDialog = document.querySelector('[data-confirm-dialog]');
  let pendingDeleteForm = null;
  document.querySelectorAll('[data-confirm-delete]').forEach(button => button.addEventListener('click', () => {
    pendingDeleteForm = document.getElementById(button.dataset.deleteForm);
    const title = confirmDialog?.querySelector('[data-confirm-title]');
    const message = confirmDialog?.querySelector('[data-confirm-message]');
    if (title) title.textContent = `Delete ${button.dataset.deleteName}?`;
    if (message) message.textContent = button.dataset.deleteMessage;
    confirmDialog?.showModal();
  }));
  confirmDialog?.querySelector('[data-confirm-cancel]')?.addEventListener('click', () => { pendingDeleteForm = null; confirmDialog.close(); });
  confirmDialog?.querySelector('[data-confirm-submit]')?.addEventListener('click', () => pendingDeleteForm?.requestSubmit());
  confirmDialog?.addEventListener('click', event => { if (event.target === confirmDialog) confirmDialog.close(); });

  const iconDialog = document.querySelector('[data-icon-picker]');
  const iconOptions = [...document.querySelectorAll('[data-icon-option]')];
  const iconSearch = iconDialog?.querySelector('[data-icon-search]');
  let iconGroup = 'all';
  const iconMarkup = option => `<span class="technology-icon" style="--technology-color:${option.dataset.color};--technology-size:42px" aria-hidden="true"><img src="https://cdn.simpleicons.org/${option.dataset.slug}/${option.dataset.color.replace('#', '')}" alt="" data-tech-icon-image><span class="technology-icon-fallback">${option.dataset.name.charAt(0)}</span></span>`;
  const filterIcons = () => {
    const query = iconSearch?.value.trim().toLowerCase() || '';
    let visible = 0;
    iconOptions.forEach(option => {
      const match = (!query || option.dataset.search.includes(query)) && (iconGroup === 'all' || option.dataset.group === iconGroup);
      option.hidden = !match;
      if (match) visible++;
    });
    const empty = iconDialog?.querySelector('[data-icon-picker-empty]');
    if (empty) empty.hidden = visible > 0;
  };
  const applyIcon = option => {
    if (!option) return;
    const form = document.querySelector('.technology-editor-form');
    const slug = form?.querySelector('[data-icon-slug]');
    const color = form?.querySelector('[data-icon-color]');
    if (slug) slug.value = option.dataset.slug;
    if (color) color.value = option.dataset.color;
    iconOptions.forEach(item => item.classList.toggle('selected', item === option));
    document.querySelectorAll('[data-selected-icon], [data-preview-icon]').forEach(target => target.innerHTML = iconMarkup(option));
    const selectedName = document.querySelector('[data-selected-icon-name]');
    if (selectedName) selectedName.textContent = option.dataset.name;
    refreshIcons();
  };
  document.querySelector('[data-open-icon-picker]')?.addEventListener('click', () => { iconDialog?.showModal(); setTimeout(() => iconSearch?.focus(), 30); });
  iconDialog?.querySelector('[data-close-icon-picker]')?.addEventListener('click', () => iconDialog.close());
  iconDialog?.addEventListener('click', event => { if (event.target === iconDialog) iconDialog.close(); });
  iconOptions.forEach(option => option.addEventListener('click', () => { applyIcon(option); iconDialog?.close(); }));
  iconSearch?.addEventListener('input', filterIcons);
  iconDialog?.querySelectorAll('[data-icon-filter]').forEach(button => button.addEventListener('click', () => {
    iconGroup = button.dataset.iconFilter;
    iconDialog.querySelectorAll('[data-icon-filter]').forEach(item => item.classList.toggle('selected', item === button));
    filterIcons();
  }));

  const technologyName = document.querySelector('[data-technology-name]');
  const technologyDescription = document.querySelector('[data-technology-description]');
  const suggestion = document.querySelector('[data-icon-suggestion]');
  let suggestedOption = null;
  const updateTechnologyPreview = () => {
    const name = technologyName?.value.trim() || 'Technology name';
    const description = technologyDescription?.value.trim() || 'Short description';
    const previewName = document.querySelector('[data-preview-name]');
    const previewDescription = document.querySelector('[data-preview-description]');
    if (previewName) previewName.textContent = name;
    if (previewDescription) previewDescription.textContent = description;
    const query = technologyName?.value.trim().toLowerCase() || '';
    suggestedOption = query.length > 1 ? iconOptions.find(option => option.dataset.name.toLowerCase() === query || option.dataset.search.split(' ').includes(query) || option.dataset.search.includes(query)) : null;
    if (suggestion) {
      suggestion.hidden = !suggestedOption;
      if (suggestedOption) {
        suggestion.querySelector('[data-suggestion-icon]').innerHTML = iconMarkup(suggestedOption);
        suggestion.querySelector('[data-suggestion-name]').textContent = suggestedOption.dataset.name;
      }
    }
  };
  technologyName?.addEventListener('input', updateTechnologyPreview);
  technologyDescription?.addEventListener('input', updateTechnologyPreview);
  suggestion?.querySelector('[data-use-suggestion]')?.addEventListener('click', () => { applyIcon(suggestedOption); suggestion.hidden = true; });
  updateTechnologyPreview();

  const socialIconDialog = document.querySelector('[data-social-icon-picker]');
  const socialIconOptions = [...document.querySelectorAll('[data-social-icon-option]')];
  const socialIconSearch = socialIconDialog?.querySelector('[data-social-icon-search]');
  const filterSocialIcons = () => {
    const query = socialIconSearch?.value.trim().toLowerCase() || '';
    let visible = 0;
    socialIconOptions.forEach(option => {
      const matches = !query || option.dataset.search.includes(query);
      option.hidden = !matches;
      if (matches) visible++;
    });
    const empty = socialIconDialog?.querySelector('[data-social-icon-empty]');
    if (empty) empty.hidden = visible > 0;
  };
  const applySocialIcon = option => {
    const form = document.querySelector('.social-editor-form');
    const input = form?.querySelector('[data-social-icon-value]');
    if (!option || !input) return;
    input.value = option.dataset.icon;
    const preview = form.querySelector('[data-selected-social-icon]');
    const name = form.querySelector('[data-selected-social-icon-name]');
    if (preview) preview.innerHTML = `<i data-lucide="${option.dataset.icon}"></i>`;
    if (name) name.textContent = option.dataset.name;
    socialIconOptions.forEach(item => item.classList.toggle('selected', item === option));
    refreshIcons();
  };
  document.querySelector('[data-open-social-icon-picker]')?.addEventListener('click', () => {
    socialIconDialog?.showModal();
    setTimeout(() => socialIconSearch?.focus(), 30);
  });
  socialIconDialog?.querySelector('[data-close-social-icon-picker]')?.addEventListener('click', () => socialIconDialog.close());
  socialIconDialog?.addEventListener('click', event => { if (event.target === socialIconDialog) socialIconDialog.close(); });
  socialIconOptions.forEach(option => option.addEventListener('click', () => { applySocialIcon(option); socialIconDialog?.close(); }));
  socialIconSearch?.addEventListener('input', filterSocialIcons);

  document.querySelectorAll('[data-preserve-form]').forEach(form => {
    const identifier = form.querySelector('[name="Id"]')?.value || 'new';
    const storageKey = `portfolio-admin-draft:${form.dataset.preserveForm}:${identifier}`;
    // Edit screens must reflect the latest saved record. Restoring an old edit
    // draft can otherwise clear a category that was valid when the page loaded.
    if (identifier !== 'new') {
      sessionStorage.removeItem(storageKey);
      return;
    }
    try {
      const saved = JSON.parse(sessionStorage.getItem(storageKey) || 'null');
      if (saved) Object.entries(saved).forEach(([name, value]) => {
        const field = form.elements.namedItem(name);
        if (!field || name === '__RequestVerificationToken' || name === 'Id') return;
        const checkbox = form.querySelector(`input[type="checkbox"][name="${CSS.escape(name)}"]`);
        if (checkbox) checkbox.checked = Boolean(value); else field.value = value;
      });
      form.addEventListener('input', () => {
        const values = {};
        new FormData(form).forEach((value, name) => { if (name !== '__RequestVerificationToken' && name !== 'Id') values[name] = value; });
        form.querySelectorAll('input[type="checkbox"]').forEach(field => values[field.name] = field.checked);
        sessionStorage.setItem(storageKey, JSON.stringify(values));
      });
      form.addEventListener('submit', () => sessionStorage.removeItem(storageKey));
    } catch { sessionStorage.removeItem(storageKey); }
  });

  document.querySelectorAll('[data-rich-text-editor]').forEach(editor => {
    const surface = editor.querySelector('[data-rte-surface]');
    const input = editor.querySelector('[data-rte-input]');
    const toolbar = editor.querySelector('.word-editor-toolbar');
    const wordCount = editor.querySelector('[data-rte-word-count]');
    let savedRange = null;

    const rememberSelection = () => {
      const selection = window.getSelection();
      if (!selection?.rangeCount) return;
      const range = selection.getRangeAt(0);
      if (surface.contains(range.commonAncestorContainer)) savedRange = range.cloneRange();
    };
    const restoreSelection = () => {
      surface.focus();
      if (!savedRange) return;
      const selection = window.getSelection();
      selection.removeAllRanges();
      selection.addRange(savedRange);
    };
    const sync = () => {
      input.value = surface.innerHTML.trim();
      const count = surface.innerText.trim().split(/\s+/).filter(Boolean).length;
      wordCount.textContent = `${count} ${count === 1 ? 'word' : 'words'}`;
      surface.classList.toggle('is-empty', count === 0);
    };
    const runCommand = (command, value = null) => {
      restoreSelection();
      document.execCommand('styleWithCSS', false, true);
      const applied = document.execCommand(command, false, value);
      if (command === 'hiliteColor' && !applied) document.execCommand('backColor', false, value);
      rememberSelection();
      sync();
      updateToolbarState();
    };
    const updateToolbarState = () => {
      toolbar.querySelectorAll('[data-rte-command]').forEach(button => {
        const stateful = ['bold', 'italic', 'underline', 'strikeThrough', 'insertUnorderedList', 'insertOrderedList', 'justifyLeft', 'justifyCenter', 'justifyRight'].includes(button.dataset.rteCommand);
        if (!stateful) return;
        const active = document.queryCommandState(button.dataset.rteCommand);
        button.classList.toggle('active', active);
        button.setAttribute('aria-pressed', String(active));
      });
    };
    const cleanPastedHtml = html => {
      const template = document.createElement('template');
      template.innerHTML = html;
      template.content.querySelectorAll('script,style,link,meta,iframe,object,embed').forEach(element => element.remove());
      template.content.querySelectorAll('*').forEach(element => {
        [...element.attributes].forEach(attribute => {
          const name = attribute.name.toLowerCase();
          if (name.startsWith('on') || name === 'id' || name === 'class' || name.startsWith('data-')) element.removeAttribute(attribute.name);
        });
        if (element.hasAttribute('style')) {
          const allowed = ['color', 'background-color', 'text-align', 'font-weight', 'font-style', 'text-decoration', 'font-family', 'font-size'];
          [...element.style].forEach(property => { if (!allowed.includes(property)) element.style.removeProperty(property); });
          if (!element.getAttribute('style')?.trim()) element.removeAttribute('style');
        }
        if (element.tagName === 'A') {
          const href = element.getAttribute('href') || '';
          if (/^javascript:/i.test(href)) element.removeAttribute('href');
          element.setAttribute('rel', 'noreferrer');
        }
        if (element.tagName === 'IMG' && /^data:/i.test(element.getAttribute('src') || '')) element.remove();
      });
      return template.innerHTML;
    };

    toolbar.querySelectorAll('button').forEach(button => button.addEventListener('mousedown', event => event.preventDefault()));
    toolbar.querySelectorAll('[data-rte-command]').forEach(button => button.addEventListener('click', () => runCommand(button.dataset.rteCommand)));
    toolbar.querySelector('[data-rte-block]')?.addEventListener('pointerdown', rememberSelection);
    toolbar.querySelector('[data-rte-block]')?.addEventListener('change', event => runCommand('formatBlock', event.target.value));
    toolbar.querySelectorAll('[data-rte-value-command]').forEach(control => {
      control.addEventListener('pointerdown', rememberSelection);
      control.addEventListener('change', () => runCommand(control.dataset.rteValueCommand, control.value));
    });
    toolbar.querySelectorAll('[data-rte-color]').forEach(control => {
      control.addEventListener('pointerdown', rememberSelection);
      control.addEventListener('input', () => {
        control.closest('.editor-color-control')?.style.setProperty('--editor-color', control.value);
        runCommand(control.dataset.rteColor, control.value);
      });
    });
    toolbar.querySelector('[data-rte-link]')?.addEventListener('click', () => {
      rememberSelection();
      let url = window.prompt('Enter the link URL:', 'https://');
      if (!url) return;
      url = url.trim();
      if (!/^(https?:|mailto:|\/|#)/i.test(url)) url = `https://${url}`;
      runCommand('createLink', url);
      const selection = window.getSelection();
      const parent = selection?.anchorNode?.parentElement?.closest('a');
      if (parent) parent.setAttribute('rel', 'noreferrer');
      sync();
    });
    surface.addEventListener('input', sync);
    surface.addEventListener('keyup', () => { rememberSelection(); updateToolbarState(); });
    surface.addEventListener('mouseup', () => { rememberSelection(); updateToolbarState(); });
    surface.addEventListener('focus', rememberSelection);
    surface.addEventListener('paste', event => {
      const html = event.clipboardData?.getData('text/html');
      if (!html) return;
      event.preventDefault();
      runCommand('insertHTML', cleanPastedHtml(html));
    });
    surface.closest('form')?.addEventListener('submit', sync);
    sync();
  });
});
