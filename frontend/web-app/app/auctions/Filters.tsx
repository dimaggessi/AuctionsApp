import { Button, ButtonGroup } from 'flowbite-react';
import { useParams } from 'next/navigation';
import React from 'react'
import { useParamsStore } from '../hooks/useParamsStore';

const pageSizeButtons = [4, 8, 12];

export default function Filters() {
    const pageSize = useParamsStore(state => state.pageSize);
    const setParams = useParamsStore(state => state.setParams);
    return (
        <div className="flex items-center mb4">
            <span className="uppercase text-sm text-gray-500 mr-2">Page size</span>
            <ButtonGroup>
                {pageSizeButtons.map((value, index) => (
                    <Button key={index}
                        onClick={() => setParams({pageSize: value})}
                        color={`${pageSize === value ? 'red' : 'gray'}`}
                        className='focus:ring-0'
                    >
                        {value}
                    </Button>
                ))}
            </ButtonGroup>
        </div>
    )
}
