'use client'

import React from 'react'
import { AiOutlineCar } from 'react-icons/ai'
import { useParamsStore } from '../hooks/useParamsStore'

export default function Logo() {
    const reset = useParamsStore(state => state.reset);

    return (
        <div onClick={reset} className='icon-container'>
            <AiOutlineCar size={34} />
            <div>Car Auctions</div>
        </div>
    )
}