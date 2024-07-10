import React from "react";
import { AiOutlineCar } from "react-icons/ai";

// each component inside App folder will be rendered on server side

export default function Navbar() {
    // console.log("Server component");
    return (
        <header className='navbar-container'>
            <div className='icon-container'>
                <AiOutlineCar size={34}/>
                <div>Car Auctions</div> 
            </div>
            <div>Middle</div>
            <div>Right</div>
        </header>
    )
}