package com.sii.datagovenpad.adaptor.recyclerview

import android.content.Context
import android.view.LayoutInflater

/**
 * Created by zhy on 16/4/9.
 */
abstract class CommonAdapter<T>(mContext: Context, mLayoutId: Int, mDatas: List<T>) : MultiItemTypeAdapter<T>(mContext, mDatas) {
    protected var mInflater: LayoutInflater

    init {
        mInflater = LayoutInflater.from(mContext)

        addItemViewDelegate(object : ItemViewDelegate<T> {
            override val itemViewLayoutId: Int
                get() = mLayoutId

            override fun isForViewType(item: T, position: Int): Boolean {
                return true
            }

            override fun convert(holder: ViewHolder, t: T, position: Int) {
                this@CommonAdapter.convert(holder, t, position)
            }
        })
    }

    protected abstract fun convert(holder: ViewHolder, t: T, position: Int)


}
