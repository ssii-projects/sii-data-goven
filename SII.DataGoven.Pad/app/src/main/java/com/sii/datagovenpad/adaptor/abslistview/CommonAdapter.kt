package com.sii.datagovenpad.adaptor.abslistview

import android.content.Context

abstract class CommonAdapter<T>(context: Context, layoutId: Int,val datas: List<T>) : MultiItemTypeAdapter<T>(context, datas) {

    init {

        addItemViewDelegate(object : ItemViewDelegate<T> {
            override val itemViewLayoutId: Int
                get() = layoutId

            override fun isForViewType(item: T, position: Int): Boolean {
                return true
            }

            override fun convert(holder: ViewHolder, t: T, position: Int) {
                this@CommonAdapter.convert(holder, t, position)
            }
        })
    }

    protected  abstract fun convert(viewHolder: ViewHolder, item: T, position: Int)

}