// This code is in the public domain -- castanyo@yahoo.es

#ifndef NV_MESH_WELD_H
#define NV_MESH_WELD_H

//#include "nvcore/Array.h"
//#include "nvcore/Hash.h"
#include "nvcore/Utils.h" // nextPowerOfTwo

#include <algorithm>
#include <vector>
#include <string.h> // for memset, memcmp, memcpy

// Weld function to remove array duplicates in linear time using hashing.

namespace nv
{

/// Generic welding routine. This function welds the elements of the array p
/// and returns the cross references in the xrefs array. To compare the elements
/// it uses the given hash and equal functors.
/// 
/// This code is based on the ideas of Ville Miettinen and Pierre Terdiman.
template <class T, class H=std::hash<T>, class E=std::equal_to<T> >
struct Weld
{

#define NIL uint32_t(~0)

	// xrefs maps old elements to new elements
	uint32_t operator()(std::vector<T> & p, std::vector<uint32_t> & xrefs)
	{
		const uint32_t N = p.size();							// # of input vertices.
		uint32_t outputCount = 0;								// # of output vertices
		uint32_t hashSize = nextPowerOfTwo(N);					// size of the hash table
		uint32_t * hashTable = new uint32_t[hashSize + N];			// hash table + linked list
		uint32_t * next = hashTable + hashSize;					// use bottom part as linked list

		xrefs.resize(N);
		memset( hashTable, NIL, hashSize*sizeof(uint32_t) );	// init hash table (NIL = 0xFFFFFFFF so memset works)

		H hash;
		E equal;
		for (uint32_t i = 0; i < N; i++)
		{
			const T & e = p[i];
            uint32_t hashValue = hash(e) & (hashSize-1);
            uint32_t offset = hashTable[hashValue];

			// traverse linked list
			while( offset != (~0) && !equal(p[offset], e) )
			{
				offset = next[offset];
			}

			xrefs[i] = offset;

			// no match found - copy vertex & add to hash
			if( offset == NIL )
			{
				// save xref
				xrefs[i] = outputCount;

				// copy element
				p[outputCount] = e;

				// link to hash table
				next[outputCount] = hashTable[hashValue];

				// update hash heads and increase output counter
				hashTable[hashValue] = outputCount++;
			}
		}

		// cleanup
		delete[] hashTable;

		p.resize(outputCount);
		
		// number of output vertices
		return outputCount;
	}
};


/// Reorder the given array accoding to the indices given in xrefs.
template <class T>
void reorderArray(std::vector<T> & array, const std::vector<uint32_t> & xrefs)
{
	const uint32_t count = xrefs.count();
	std::vector<T> new_array;
    new_array.resize(count);

	for(uint32_t i = 0; i < count; i++) {
		new_array[i] = array[xrefs[i]];
	}

	std::swap(array, new_array);
}

/// Reverse the given array so that new indices point to old indices.
inline void reverseXRefs(std::vector<uint32_t> & xrefs, uint32_t count)
{
	std::vector<uint32_t> new_xrefs;
    new_xrefs.resize(count);
	
	for(uint32_t i = 0; i < xrefs.size(); i++) {
		new_xrefs[xrefs[i]] = i;
	}
	
	std::swap(xrefs, new_xrefs);
}



//
struct WeldN
{
    uint32_t vertexSize;

    inline uint32_t sdbmHash(const void * data_in, uint32_t size, uint32_t h = 5381)
    {
        const uint8_t * data = (const uint8_t *)data_in;
        uint32_t i = 0;
        while (i < size) {
            h = (h << 16) + (h << 6) - h + (uint32_t)data[i++];
        }
        return h;
    }

    WeldN(uint32_t n) : vertexSize(n) {}

	// xrefs maps old elements to new elements
    uint32_t operator()(uint8_t * ptr, uint32_t N, std::vector<uint32_t> & xrefs)
	{
		uint32_t outputCount = 0;								// # of output vertices
		uint32_t hashSize = nextPowerOfTwo(N);					// size of the hash table
		uint32_t * hashTable = new uint32_t[hashSize + N];			// hash table + linked list
		uint32_t * next = hashTable + hashSize;					// use bottom part as linked list

		xrefs.resize(N);
		memset( hashTable, NIL, hashSize*sizeof(uint32_t) );	// init hash table (NIL = 0xFFFFFFFF so memset works)

		for (uint32_t i = 0; i < N; i++)
		{
			const uint8_t * vertex = ptr + i * vertexSize;
            uint32_t hashValue = sdbmHash(vertex, vertexSize) & (hashSize-1);
            uint32_t offset = hashTable[hashValue];

			// traverse linked list
			while (offset != NIL && memcmp(ptr + offset * vertexSize, vertex, vertexSize) != 0)
			{
				offset = next[offset];
			}

			xrefs[i] = offset;

			// no match found - copy vertex & add to hash
			if (offset == NIL)
			{
				// save xref
				xrefs[i] = outputCount;

				// copy element
                memcpy(ptr + outputCount * vertexSize, vertex, vertexSize);

				// link to hash table
				next[outputCount] = hashTable[hashValue];

				// update hash heads and increase output counter
				hashTable[hashValue] = outputCount++;
			}
		}

		// cleanup
		delete [] hashTable;

		// number of output vertices
		return outputCount;
	}
};


} // nv namespace

#endif // NV_MESH_WELD_H
