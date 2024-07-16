'use client'

import React from 'react'
import { AiOutlineCar } from 'react-icons/ai'
import { useParamsStore } from '../hooks/useParamsStore'
import { usePathname, useRouter } from 'next/navigation'

export default function Logo() {
    const router = useRouter();
    const pathname = usePathname();
    const reset = useParamsStore(state => state.reset);

    function doReset() {
        if (pathname !== '/') router.push('/');
        reset();
    }

    return (
        <div onClick={doReset} className='icon-container'>
            <AiOutlineCar size={34} />
            <div>Car Auctions</div>
        </div>
    )
}