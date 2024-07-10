import Image from 'next/image'
import React from 'react'

type Props = {
    auction: any
}

// priority => attempt to load image soon as possible without lazy loading

export default function AuctionCard({auction}: Props) {
  return (
    <a href="#">
          <div className='auction-card-container'>
            <div>
              <Image
                  src={auction.imageUrl}
                  alt='image'
                  fill
                  priority
                  className='object-cover'
                  sizes='(max-width:768px) 100vw, (max-width: 1200px) 50vw, 25 vw'
              />
            </div>
          </div>
          <div className='flex justify-between items-center mt-4'>
            <h3 className="text-gray-700">{auction.make} {auction.model}</h3>
            <p className="font-semibold text-sm">{auction.year}</p>
          </div>
      </a>
  )
}
