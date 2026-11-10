mergeInto(LibraryManager.library, {
  AquaGuardianResult: function (json) {
    try { window.dispatchEvent(new CustomEvent('aquaguardian:result', {detail: JSON.parse(UTF8ToString(json))})); }
    catch (error) { console.error('Could not send AquaGuardian result', error); }
  }
});
