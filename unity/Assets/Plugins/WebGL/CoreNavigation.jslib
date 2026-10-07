mergeInto(LibraryManager.library, {
  CampusReturnToCore: function(url) {
    window.location.assign(UTF8ToString(url));
  }
});
