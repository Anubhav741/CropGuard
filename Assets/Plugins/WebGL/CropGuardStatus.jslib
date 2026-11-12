mergeInto(LibraryManager.library, {
  CropGuardSpeak: function(message) { window.dispatchEvent(new CustomEvent('cropguard-speak', { detail: UTF8ToString(message) })); },
  CropGuardStatus: function(message) {
    window.dispatchEvent(new CustomEvent('cropguard-status', { detail: UTF8ToString(message) }));
  }
});
