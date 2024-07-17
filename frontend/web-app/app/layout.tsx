import type { Metadata } from "next";
import Navbar from "./nav/Navbar";
import './globals.css'
import ToasterProvider from "./providers/ToasterProvider";

export const metadata: Metadata = {
  title: "Auctions App",
  description: "Generated by create next app",
};

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html lang="en">
      <body>
        <ToasterProvider />
        <Navbar />
        <main className='main-container'>
          {children}
        </main>
      </body>
    </html>
  );
}
