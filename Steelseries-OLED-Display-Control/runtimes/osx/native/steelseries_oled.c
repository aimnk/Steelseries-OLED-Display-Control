#include <stdio.h>
#include <unistd.h>
#include <hidapi/hidapi.h>

#ifdef __cplusplus
extern "C" {
#endif

int disable_steelseries_ui()
{
    if (hid_init() != 0)
        return -1;

    struct hid_device_info* devs = hid_enumerate(0x1038, 0x12E0);
    struct hid_device_info* cur = devs;
    int success = 0;

    while (cur) {
        if (cur->interface_number == 4) {
            printf("[native] Trying path: %s (interface %d)\n", cur->path, cur->interface_number);
            hid_device* handle = hid_open_path(cur->path);
            if (handle) {
                unsigned char report[64] = {0x06, 0x81};

                for (int attempt = 0; attempt < 5; ++attempt) {
                    int res = hid_write(handle, report, sizeof(report));
                    printf("[native] hid_write attempt %d → %d\n", attempt + 1, res);

                    if (res >= 0) {
                        usleep(100000); // 100ms delay

                        // Try to send a blank frame to force flush
                        unsigned char blank[64] = {0};
                        hid_write(handle, blank, sizeof(blank));

                        success = 1;
                        break;
                    }
                    usleep(200000); // 200ms between attempts
                }

                hid_close(handle);
                if (success) break;
            } else {
                printf("[native] Failed to open device.\n");
            }
        }
        cur = cur->next;
    }

    hid_free_enumeration(devs);
    hid_exit();
    return success ? 1 : -2;
}

#ifdef __cplusplus
}
#endif