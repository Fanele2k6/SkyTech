//tiny interactive script for scanner buttons and bottom nav active state(demo)
  
        (function() {
      // top scanner
      const topScanner = document.querySelector('.scanner-btn');
        if (topScanner) {
            topScanner.addEventListener('click', () => alert('📷 Top scanner activated!'));
      }
        // bottom scan button
        const bottomScan = document.getElementById('bottomScanBtn');
        if (bottomScan) {
            bottomScan.addEventListener('click', (e) => {
                e.preventDefault();
                alert('📱 Bottom scan – ready to scan barcode');
            });
      }
        // simulate active state for bottom nav (just demo)
        const navLinks = document.querySelectorAll('.bottom-nav a');
      navLinks.forEach(link => {
            link.addEventListener('click', function (e) {
                navLinks.forEach(l => l.classList.remove('active'));
                this.classList.add('active');
                // prevent actual navigation for demo
                if (this.getAttribute('href') === '#') e.preventDefault();
            });
      });
    })();
  