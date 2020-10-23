setTimeout(function () {
  // dotnet-watch browser reload script
  let connection;
  try {
    connection = new WebSocket('{{hostString}}');
  } catch (ex) {
    console.debug(ex);
    return;
  }
  connection.onmessage = function (message) {
    const updateCSSMessage = 'UpdateCSS';
    if (message.data === 'Reload') {
      console.debug('Server is ready. Reloading...');
      location.reload();
    } else if (message.data === 'Wait') {
      console.debug('File changes detected. Waiting for application to rebuild.');
      const t = document.title; const r = ['☱', '☲', '☴']; let i = 0;
      setInterval(function () { document.title = r[i++ % r.length] + ' ' + t; }, 240);
    } else if (message.data.startsWith(updateCSSMessage)) {
      const index = message.data.indexOf('||', updateCSSMessage);
      const fileName = message.data.substring(updateCSSMessage.length, index);
      const fileContent = message.data.substring(index + 2);

      let cssUpdated = false;
      for (let i = 0; i < document.styleSheets.length; i++) {
        if (document.styleSheets[i].href.endsWith(fileName)) {
          const initialStyleSheet = document.styleSheets[i];

          for (let i = 0; i < initialStyleSheet.rules.length; i++) {
            initialStyleSheet.deleteRule(i);
          }

          const styleSheet = new CSSStyleSheet();
          styleSheet.replaceSync(fileContent);
          for (let i = 0; i < styleSheet.rules.length; i++) {
            initialStyleSheet.insertRule(styleSheet.rules[i].cssText);
          }
          cssUpdated = true;
          break;
        }
      }
      if (!cssUpdated) {
        location.reload();
      }
    }
  }

  connection.onerror = function (event) { console.debug('dotnet-watch reload socket error.', event) }
  connection.onclose = function () { console.debug('dotnet-watch reload socket closed.') }
  connection.onopen = function () { console.debug('dotnet-watch reload socket connected.') }
}, 500);
