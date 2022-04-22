"""Utilities for parsing the images."""
import cv2
# Dependency imports
import numpy as np

# The heatmap colorization value in OpenCV
HEAT_COLORMAP = cv2.COLORMAP_JET


def scale(rad_evo_thermal, upsample_ratio=None, v_flip=False):
    """Decode the input radiometry frame of the EVO THERMAL camera as grayscale image.

    If the upsample_ratio set it will return an upsampled image with the ration equal
    to that value and if the v_flip flag is True the returned grayscale images will be
    vertically flipped.

    Args:
        rad_evo_thermal: the raw thermal stitched image from EVO THERMAL cameras.
        upsample_ratio: ratio value to upsample the returned images with.
        v_flip: flag to indicate whether to flip the returned image
        vertically or not.

    Returns:
        The grayscale image from the raw thermal stitched image.
    """
    grayscale_img = np.zeros_like(rad_evo_thermal)
    # Normalize the raw radiometry frame values between 0..255
    cv2.normalize(rad_evo_thermal, grayscale_img, 0, 255, cv2.NORM_MINMAX)
    # Resize the output grayscale image with the upsample factor if exist
    if upsample_ratio is not None:
        grayscale_img = cv2.resize(grayscale_img, (upsample_ratio * grayscale_img.shape[1],
                                                   upsample_ratio * grayscale_img.shape[0]),
                                   interpolation=cv2.INTER_NEAREST)

    grayscale_img = grayscale_img.astype(np.uint8)

    # Flip the returned grayscale image vertically if True
    if v_flip:
        cv2.flip(grayscale_img, 0, grayscale_img)
    return grayscale_img


def decode_as_heatmap(grayscale_evo_thermal):
    """Decode the input grayscale thermal image as a colorized heatmap image.

    Args:
       grayscale_evo_thermal: the grayscale image of the stitched two EVO THERMAL camera frames.

    Returns:
        The colorized grayscale image in heat color map.
    """
    # Create a placeholder image equal in size to the input grayscale image with 3 channels (R,G,B)
    heatmap_evo_thermal = np.zeros(
        (grayscale_evo_thermal.shape[0], grayscale_evo_thermal.shape[1], 3), dtype=np.uint8)
    # Add new axis to the numpy array of the grayscale image, since it's only 2D
    heatmap_evo_thermal[:, :, :] = grayscale_evo_thermal[..., np.newaxis]
    # Apply the heatmap clorization to the grayscale image
    heatmap_evo_thermal = cv2.applyColorMap(heatmap_evo_thermal, HEAT_COLORMAP)
    return heatmap_evo_thermal
