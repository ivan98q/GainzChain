#include<world.h>

#include <random.h>


std::vector<std::vector<std::vector<int>>> GenerateWorld(uint256 prevBlockHash)
{
    std::vector<std::vector<std::vector<int>>> world;
    world.resize(16);

    FastRandomContext rand(prevBlockHash);
    for (unsigned int x = 0; x < world.size(); ++x) {
        world[x].resize(16);
        for (unsigned int y = 0; y < world[x].size(); ++y) {
            world[x][y].resize(16);
            for (unsigned int z = 0; z < world[x][y].size(); ++z) {
                if (z < 4) {
                    continue;
                }
                uint64_t res = rand.randrange(10);
                if (res == 0) {
                    world[x][y][z] = 1;
                } else {
                    world[x][y][z] = 0;
                }
            }
        }
    }

    return world;
}
