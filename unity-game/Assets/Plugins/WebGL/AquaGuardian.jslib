mergeInto(LibraryManager.library, {
  AquaGuardianResult: function (json) {
    var result = JSON.parse(UTF8ToString(json));
    window.dispatchEvent(new CustomEvent('aquaguardian:result', { detail: result }));
  }
});
