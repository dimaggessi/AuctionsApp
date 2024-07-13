import React from "react";
import Search from "./Search";
import Logo from "./Logo";

// each component inside App folder will be rendered on server side

export default function Navbar() {
    // console.log("Server component");
    return (
        <header className='navbar-container'>
            <Logo />
            <Search />
            <div>Login</div>
        </header>
    )
}