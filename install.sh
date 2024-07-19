# Setup
sudo yum install lynx

# Usage
lynx -dump https://en.wikipedia.org/wiki/Albert_Einstein | less
lynx -dump https://en.wikipedia.org/wiki/Albert_Einstein | wc -l
lynx -dump https://en.wikipedia.org/wiki/Albert_Einstein | wc --bytes
lynx -dump https://en.wikipedia.org/wiki/Albert_Einstein | head -c 5000

TEXT=`lynx -dump https://en.wikipedia.org/wiki/Albert_Einstein | head -c 5000`
aws comprehend detect-sentiment --language-code "en" --text "$TEXT"
aws comprehend detect-sentiment --language-code "en" --text "$TEXT" --output text | head -n 25