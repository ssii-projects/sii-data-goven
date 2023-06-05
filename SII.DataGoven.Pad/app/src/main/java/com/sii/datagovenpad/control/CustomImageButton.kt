package com.sii.datagovenpad.control

import android.content.Context
import android.util.AttributeSet

class CustomImageButton(context: Context, attrs: AttributeSet): androidx.appcompat.widget.AppCompatImageButton(context, attrs) {
    override fun setEnabled(enabled:Boolean){
        if (this.isEnabled !== enabled) {
            this.imageAlpha = if (enabled) 0xFF else 0x3F
        }
        super.setEnabled(enabled)
    }
}