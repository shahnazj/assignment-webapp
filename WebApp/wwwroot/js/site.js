// Main initialization
document.addEventListener("DOMContentLoaded", () => {
  initQuills();
  initModals();
  initDropdowns();
  initFileUploads();
  initCustomSelects();
  initDarkMode();
  initCookieConsent();
  initProjectHandlers();
  initMemberHandlers();
  initClientHandlers();
});

// Rich Text Editor Initialization
function initQuills() {
  document.querySelectorAll("[data-quill-editor]").forEach((editor) => {
    const editorId = editor.id;
    const textarea = document.querySelector(`[data-quill-textarea="#${editorId}"]`);
    const toolbarId = editor.getAttribute("data-quill-toolbar");

    const quill = new Quill(`#${editorId}`, {
      modules: {
        syntax: true,
        toolbar: toolbarId,
      },
      theme: "snow",
      placeholder: "Skriv något...",
    });

    if (textarea) {
      quill.on("text-change", () => {
        textarea.value = quill.root.innerHTML;
      });
    }
  });
}

// Modal Management
function initModals() {
  document.querySelectorAll('[data-type="modal"]').forEach((trigger) => {
    const target = document.querySelector(trigger.getAttribute("data-target"));
    trigger.addEventListener("click", () => target?.classList.add("modal-show"));
  });

  document.querySelectorAll('[data-type="close"]').forEach((btn) => {
    const target = document.querySelector(btn.getAttribute("data-target"));
    btn.addEventListener("click", () => target?.classList.remove("modal-show"));
  });
}

// Dropdown Management
function initDropdowns() {
  document.addEventListener("click", (e) => {
    let clickedInsideDropdown = false;

    document.querySelectorAll('[data-type="dropdown"]').forEach((dropdownTrigger) => {
      const targetId = dropdownTrigger.getAttribute("data-target");
      const dropdown = document.querySelector(targetId);

      if (dropdownTrigger.contains(e.target)) {
        clickedInsideDropdown = true;
        document.querySelectorAll(".dropdown.dropdown-show").forEach((open) => {
          if (open !== dropdown) open.classList.remove("dropdown-show");
        });
        dropdown?.classList.toggle("dropdown-show");
      }
    });

    if (!clickedInsideDropdown && !e.target.closest(".dropdown")) {
      document.querySelectorAll(".dropdown.dropdown-show").forEach((open) => {
        open.classList.remove("dropdown-show");
      });
    }
  });
}

// File Upload Management
function initFileUploads() {
  document.querySelectorAll("[data-file-upload]").forEach((container) => {
    const input = container.querySelector('input[type="file"]');
    const preview = container.querySelector("img");
    const iconContainer = container.querySelector("circle");
    const icon = iconContainer?.querySelector("i");

    container.addEventListener("click", () => input?.click());

    input?.addEventListener("change", (e) => {
      const file = e.target.files[0];
      if (file && file.type.startsWith("image/")) {
        const reader = new FileReader();
        reader.onload = () => {
          preview.src = reader.result;
          preview.classList.remove("hide");
          iconContainer?.classList.add("selected");
          icon?.classList.replace("fa-camera", "fa-pen-to-square");
        };
        reader.readAsDataURL(file);
      }
    });
  });
}

// Custom Select Management
function initCustomSelects() {
  document.querySelectorAll(".form-select").forEach((select) => {
    const trigger = select.querySelector(".form-select-trigger");
    const triggerText = trigger.querySelector(".form-select-text");
    const options = select.querySelectorAll(".form-select-option");
    const hiddenInput = select.querySelector('input[type="hidden"]');
    const placeholder = select.dataset.placeholder || "Välj";

    const setValue = (value = "", text = placeholder) => {
      triggerText.textContent = text;
      hiddenInput.value = value;
      select.classList.toggle("has-placeholder", !value);
    };

    setValue();

    trigger.addEventListener("click", (e) => {
      e.stopPropagation();
      document.querySelectorAll(".form-select.open").forEach((el) => {
        if (el !== select) el.classList.remove("open");
      });
      select.classList.toggle("open");
    });

    options.forEach((option) => {
      option.addEventListener("click", () => {
        setValue(option.dataset.value, option.textContent);
        select.classList.remove("open");
      });
    });

    document.addEventListener("click", (e) => {
      if (!select.contains(e.target)) {
        select.classList.remove("open");
      }
    });
  });
}

// Dark Mode Management
function initDarkMode() {
  const darkModeToggle = document.getElementById('darkModeToggle');
  const isDarkMode = localStorage.getItem('darkMode') === 'true';
  
  if (isDarkMode) {
    document.body.classList.add('dark-mode');
    darkModeToggle.checked = true;
  }

  darkModeToggle.addEventListener('change', (e) => {
    const isDark = e.target.checked;
    document.body.classList.toggle('dark-mode', isDark);
    localStorage.setItem('darkMode', isDark);
  });
}

// Cookie Consent Management
function initCookieConsent() {
  const cookieModal = document.getElementById('cookieModal');
  if (!cookieModal) return; // Exit if cookie modal doesn't exist
  
  showCookieModal();
  
  const consentValue = getCookie('cookieConsent');
  if (consentValue) {
    try {
      const consent = JSON.parse(consentValue);
      const essentialCheckbox = document.getElementById("essential");
      const analyticsCheckbox = document.getElementById("analytics");
      const marketingCheckbox = document.getElementById("marketing");
      
      if (essentialCheckbox) essentialCheckbox.checked = consent.essential;
      if (analyticsCheckbox) analyticsCheckbox.checked = consent.analytics;
      if (marketingCheckbox) marketingCheckbox.checked = consent.marketing;
    } catch (error) {
      console.error('Unable to handle cookie consent values', error);
    }
  }
}

function showCookieModal() {
  const modal = document.getElementById('cookieModal');
  if (modal) modal.style.display = "flex";
}

function hideCookieModal() {
  const modal = document.getElementById('cookieModal');
  if (modal) modal.style.display = "none";
}

function getCookie(name) {
  const nameEQ = name + "=";
  const cookies = document.cookie.split(';');
  for (let cookie of cookies) {
    cookie = cookie.trim();
    if (cookie.indexOf(nameEQ) === 0) {
      return decodeURIComponent(cookie.substring(nameEQ.length));
    }
  }
  return null;
}

function setCookie(name, value, days) {
  let expires = "";
  if (days) {
    const date = new Date();
    date.setTime(date.getTime() + days * 24 * 60 * 60 * 1000);
    expires = "; expires=" + date.toUTCString();
  }
  document.cookie = `${name}=${encodeURIComponent(value || "")}${expires}; path=/; SameSite=Lax`;
}

async function handleConsent(consent) {
  try {
    const res = await fetch('/cookies/setcookies', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(consent)
    });
    if (!res.ok) {
      console.error("Unable to set cookie consent", await res.text());
    }
  } catch (error) {
    console.error("Error setting cookie consent:", error);
  }
}

async function acceptSelected() {
  const essentialCheckbox = document.getElementById("essential");
  const analyticsCheckbox = document.getElementById("analytics");
  const marketingCheckbox = document.getElementById("marketing");
  
  if (!essentialCheckbox || !analyticsCheckbox || !marketingCheckbox) return;
  
  const consent = {
    essential: essentialCheckbox.checked,
    analytics: analyticsCheckbox.checked,
    marketing: marketingCheckbox.checked
  };
  setCookie("cookieConsent", JSON.stringify(consent), 365);
  await handleConsent(consent);
  hideCookieModal();
}

function reopenCookieModal() {
  const consentValue = getCookie('cookieConsent');
  if (!consentValue) return;
  
  try {
    const consent = JSON.parse(consentValue);
    const essentialCheckbox = document.getElementById("essential");
    const analyticsCheckbox = document.getElementById("analytics");
    const marketingCheckbox = document.getElementById("marketing");
    
    if (essentialCheckbox) essentialCheckbox.checked = consent.essential;
    if (analyticsCheckbox) analyticsCheckbox.checked = consent.analytics;
    if (marketingCheckbox) marketingCheckbox.checked = consent.marketing;
    
    showCookieModal();
  } catch (error) {
    console.error('Unable to handle cookie consent values', error);
  }
}

// Project Management
function initProjectHandlers() {
  // Project deletion
  $(document).on('click', '.project .remove', handleProjectDelete);
  
  // Project form submission
  $(document).on('submit', '#add-project-form', handleAddProject);
  $(document).on('submit', '#edit-project-form', handleEditProject);
  
  // Edit project button click
  $(document).on('click', '[data-type="modal"][data-target="#edit-project-modal"]', handleEditProjectClick);
}

function handleProjectDelete(e) {
  e.preventDefault();
  const projectId = $(this).closest('.project').data('project-id');
  if (confirm('Are you sure you want to delete this project?')) {
    $.ajax({
      url: '/admin/projects/delete/' + projectId,
      type: 'POST',
      data: { __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val() },
      success: () => location.reload(),
      error: () => alert('Error deleting project.')
    });
  }
}

function handleAddProject(e) {
  e.preventDefault();
  $('.text-danger').text('');
  const formData = new FormData(this);
  formData.set('Description', $('#add-project-description-wysiwyg-editor').find('.ql-editor').html());
  
  $.ajax({
    url: $(this).attr('action'),
    type: 'POST',
    data: formData,
    processData: false,
    contentType: false,
    success: handleProjectFormSuccess,
    error: handleProjectFormError
  });
}

function handleEditProject(e) {
  e.preventDefault();
  $('.text-danger').text('');
  const formData = new FormData(this);
  formData.delete('__Invariant');
  formData.set('Description', $('#edit-project-description-wysiwyg-editor').find('.ql-editor').html());
  
  $.ajax({
    url: '/admin/projects/edit',
    type: 'POST',
    data: formData,
    processData: false,
    contentType: false,
    success: handleProjectFormSuccess,
    error: handleProjectFormError
  });
}

function handleProjectFormSuccess(result) {
  if (result.success) {
    $('#edit-project-modal, #add-project-modal').removeClass('modal-show');
    location.reload();
  } else if (result.errors?.length > 0) {
    result.errors.forEach(error => {
      $('<span class="text-danger"></span>').text(error).appendTo('.form-group:first');
    });
  }
}

function handleProjectFormError(xhr, status, error) {
  console.error('Error submitting form:', error);
  alert('Error processing project. Please try again.');
}

function handleEditProjectClick() {
  const projectId = $(this).closest('.project').data('project-id');
  $('#edit-project-modal').addClass('modal-show');
  
  const quillContainer = $('#edit-project-description-wysiwyg-editor');
  let quillEditor = quillContainer[0].__quill;
  
  if (!quillEditor) {
    setTimeout(() => {
      quillEditor = new Quill('#edit-project-description-wysiwyg-editor', {
        modules: {
          syntax: true,
          toolbar: '#edit-project-description-toolbar'
        },
        theme: 'snow',
        placeholder: 'Skriv något...'
      });
      quillContainer[0].__quill = quillEditor;
      loadProjectData(projectId, quillEditor);
    }, 200);
  } else {
    loadProjectData(projectId, quillEditor);
  }
}

function loadProjectData(projectId, quillEditor) {
  $.ajax({
    url: '/admin/projects/get/' + projectId,
    type: 'GET',
    success: (result) => {
      const form = $('#edit-project-form');
      form.find('[name="Id"]').val(result.id);
      form.find('[name="ProjectName"]').val(result.projectName);
      form.find('[name="StartDate"]').val(result.startDate);
      form.find('[name="EndDate"]').val(result.endDate);
      form.find('[name="Budget"]').val(result.budget);
      
      updateSelectValue(form.find('.form-select').first(), result.clientId);
      updateSelectValue(form.find('.form-select').eq(1), result.memberId);
      updateSelectValue(form.find('.form-select').last(), result.statusId);
      
      if (result.projectImage) {
        const imagePreview = form.find('.image-preview-container img');
        imagePreview.attr('src', result.projectImage).removeClass('hide');
        form.find('.image-preview-container .circle').addClass('hide');
      }
      
      const descriptionTextarea = form.find('#edit-project-description');
      descriptionTextarea.val(result.description);
      quillEditor.root.innerHTML = result.description || '';
      descriptionTextarea.val(quillEditor.root.innerHTML);
      quillEditor.update();
    },
    error: () => alert('Error loading project data. Please try again.')
  });
}

function updateSelectValue(select, value) {
  const option = select.find('.form-select-option').filter(function() {
    return $(this).data('value').toLowerCase() === value.toLowerCase();
  });
  
  if (option.length) {
    select.find('.form-select-text').text(option.text());
    select.find('input[type="hidden"]').val(value);
    select.removeClass('has-placeholder');
  }
}

// Member Management
function initMemberHandlers() {
  $(document).on('click', '[data-type="modal"][data-target="#edit-member-modal"]', handleEditMemberClick);
  $(document).on('submit', '#edit-member-form', handleEditMember);
  $(document).on('click', '.member .dropdown-action.remove', handleMemberDelete);

  // Handle edit member click
  document.querySelectorAll('[data-type="modal"][data-target="#edit-member-modal"]').forEach(button => {
    button.addEventListener('click', handleEditMemberClick);
  });

  // Handle message button click
  document.querySelectorAll('#members .member.card .card-footer button').forEach(button => {
    button.addEventListener('click', () => {
      alert('Message functionality will be implemented soon!');
    });
  });
}

function handleEditMemberClick() {
  const memberId = $(this).closest('.member').data('member-id');
  $('#edit-member-modal').addClass('modal-show');
  
  $.ajax({
    url: '/admin/members/get/' + memberId,
    type: 'GET',
    success: (result) => {
      const form = $('#edit-member-form');
      form.find('[name="Id"]').val(result.id);
      form.find('[name="MemberFirstName"]').val(result.memberFirstName);
      form.find('[name="MemberLastName"]').val(result.memberLastName);
      form.find('[name="MemberEmail"]').val(result.memberEmail);
      form.find('[name="MemberPhone"]').val(result.memberPhone);
      form.find('[name="MemberJobTitle"]').val(result.memberJobTitle);
      form.find('[name="MemberAddress"]').val(result.memberAddress);
      form.find('[name="MemberBirthDate"]').val(result.memberBirthDate);
      
      if (result.memberImage) {
        const imagePreview = form.find('.image-preview-container img');
        imagePreview.attr('src', result.memberImage).removeClass('hide');
        form.find('.image-preview-container .circle').addClass('hide');
      }
    },
    error: () => alert('Error loading member data. Please try again.')
  });
}

function handleEditMember(e) {
  e.preventDefault();
  $('.text-danger').text('');
  const formData = new FormData(this);
  
  $.ajax({
    url: '/admin/members/edit/' + formData.get('Id'),
    type: 'POST',
    data: formData,
    processData: false,
    contentType: false,
    success: (result) => {
      if (result.success) {
        $('#edit-member-modal').removeClass('modal-show');
        location.reload();
      } else if (result.errors?.length > 0) {
        result.errors.forEach(error => {
          $('<span class="text-danger"></span>').text(error).appendTo('.form-group:first');
        });
      }
    },
    error: () => alert('Error updating member. Please try again.')
  });
}

function handleMemberDelete(e) {
  e.preventDefault();
  const memberId = $(this).data('id');
  
  if (confirm('Are you sure you want to delete this member?')) {
    $.ajax({
      url: '/admin/members/delete/' + memberId,
      type: 'POST',
      data: { __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val() },
      success: () => location.reload(),
      error: () => alert('Error deleting member. Please try again.')
    });
  }
}

// Client Management
function initClientHandlers() {
  $(document).on('click', '[data-type="modal"][data-target="#edit-client-modal"]', handleEditClientClick);
  $(document).on('submit', '#edit-client-form', handleEditClient);
  $(document).on('click', '.client .dropdown-action.remove', handleClientDelete);

  // Handle edit client click
  document.querySelectorAll('[data-type="modal"][data-target="#edit-client-modal"]').forEach(button => {
    button.addEventListener('click', handleEditClientClick);
  });

  // Handle view projects button click
  document.querySelectorAll('#clients .client.card .card-footer button').forEach(button => {
    button.addEventListener('click', () => {
      alert('View Projects functionality will be implemented soon!');
    });
  });
}

function handleEditClientClick() {
  const clientId = $(this).closest('.client').data('client-id');
  $('#edit-client-modal').addClass('modal-show');
  
  $.ajax({
    url: '/admin/clients/get/' + clientId,
    type: 'GET',
    success: (result) => {
      const form = $('#edit-client-form');
      form.find('[name="Id"]').val(result.id);
      form.find('[name="ClientName"]').val(result.clientName);
      form.find('[name="ClientEmail"]').val(result.clientEmail);
      form.find('[name="ClientPhone"]').val(result.clientPhone);
      form.find('[name="ClientCompany"]').val(result.clientCompany);
      form.find('[name="ClientAddress"]').val(result.clientAddress);
      
      if (result.clientImage) {
        const imagePreview = form.find('.image-preview-container img');
        imagePreview.attr('src', result.clientImage).removeClass('hide');
        form.find('.image-preview-container .circle').addClass('hide');
      }
    },
    error: () => alert('Error loading client data. Please try again.')
  });
}

function handleEditClient(e) {
  e.preventDefault();
  $('.text-danger').text('');
  const formData = new FormData(this);
  
  $.ajax({
    url: '/admin/clients/edit/' + formData.get('Id'),
    type: 'POST',
    data: formData,
    processData: false,
    contentType: false,
    success: (result) => {
      if (result.success) {
        $('#edit-client-modal').removeClass('modal-show');
        location.reload();
      } else if (result.errors?.length > 0) {
        result.errors.forEach(error => {
          $('<span class="text-danger"></span>').text(error).appendTo('.form-group:first');
        });
      }
    },
    error: () => alert('Error updating client. Please try again.')
  });
}

function handleClientDelete(e) {
  e.preventDefault();
  const clientId = $(this).data('id');
  
  if (confirm('Are you sure you want to delete this client?')) {
    $.ajax({
      url: '/admin/clients/delete/' + clientId,
      type: 'POST',
      data: { __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val() },
      success: () => location.reload(),
      error: () => alert('Error deleting client. Please try again.')
    });
  }
}
