[Generate SSL Certificates Locally]
----------------------------------------
install chocolatey package manager (on windows)
----------------------------------------
mkcert -install
cd AuctionsApp\devcerts\

key file certficate file and then uri(s)
----------------------------------------
mkcert -key-file auctionsapp.com.key -cert-file auctionsapp.com.crt app.auctionsapp.com api.auctionsapp.com id.auctionsapp.com
----------------------------------------

result:
----------------------------------------
Created a new certificate valid for the following names 📜
 - "app.auctionsapp.com"
 - "api.auctionsapp.com"
 - "id.auctionsapp.com"

The certificate is at "auctionsapp.com.crt" and the key at "auctionsapp.com.key" ✅

It will expire on 26 October 2026 🗓