/** @type {import('next').NextConfig} */
const nextConfig = {
    // tells to accept images from cdn.pixabay.com
    images: {
        domains: [
            'cdn.pixabay.com'
        ]
    },
    output: 'standalone'
};

export default nextConfig;
