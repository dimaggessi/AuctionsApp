import React from "react";
import Search from "./Search";
import Logo from "./Logo";
import LoginButton from "./LoginButton";
import { getCurrentUser } from "../actions/authActions";
import UserActions from "./UserActions";

// each component inside App folder will be rendered on server side

export default async function Navbar() {

    const user = await getCurrentUser();
    // console.log("Server component");
    return (
        <header className='navbar-container'>
            <Logo />
            <Search />
            {user ? (
                <UserActions user={user} />
            ) : (
                <LoginButton />
            )}
        </header>
    )
}