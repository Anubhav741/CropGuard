mergeInto(LibraryManager.library, {
  CropGuardStatus: function(message) {
    window.dispatchEvent(new CustomEvent('cropguard-status', { detail: UTF8ToString(message) }));
  }
});
