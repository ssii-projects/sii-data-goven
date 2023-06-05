package com.sii.datagovenpad.adaptor.abslistview

import android.content.Context
//import android.support.v4.util.SparseArrayCompat
import android.view.LayoutInflater
import android.view.View
import android.view.ViewGroup
import android.widget.BaseAdapter
import androidx.collection.SparseArrayCompat

open class MultiItemTypeAdapter<T>(protected var mContext: Context, protected var mDatas: List<T>) : BaseAdapter() {

    private val mItemViewDelegateManager= ItemViewDelegateManager<T>()



    fun addItemViewDelegate(itemViewDelegate: ItemViewDelegate<T>): MultiItemTypeAdapter<*> {
        mItemViewDelegateManager.addDelegate(itemViewDelegate)
        return this
    }

    private fun useItemViewDelegateManager(): Boolean {
        return mItemViewDelegateManager.itemViewDelegateCount > 0
    }

    override fun getViewTypeCount(): Int {
        return if (useItemViewDelegateManager()) mItemViewDelegateManager.itemViewDelegateCount else super.getViewTypeCount()
    }

    override fun getItemViewType(position: Int): Int {
        return if (useItemViewDelegateManager()) {
            mItemViewDelegateManager.getItemViewType(mDatas[position], position)
        } else super.getItemViewType(position)
    }

    override fun getView(position: Int, convertView: View?, parent: ViewGroup): View {
        val itemViewDelegate = mItemViewDelegateManager.getItemViewDelegate(mDatas[position], position)
        val layoutId = itemViewDelegate.itemViewLayoutId
        var viewHolder: ViewHolder?
        if (convertView == null) {
            val itemView = LayoutInflater.from(mContext).inflate(layoutId, parent,
                    false)
            viewHolder = ViewHolder(mContext, itemView, parent, position)
            viewHolder.layoutId = layoutId
            onViewHolderCreated(viewHolder, viewHolder.convertView)
        } else {
            viewHolder = convertView.tag as ViewHolder
            viewHolder.itemPosition = position
        }


        doConvert(viewHolder, getItem(position), position)
        return viewHolder.convertView
    }

    protected fun doConvert(viewHolder: ViewHolder, item: T, position: Int) {
        mItemViewDelegateManager.convert(viewHolder, item, position)
    }

    fun onViewHolderCreated(holder: ViewHolder, itemView: View) {}

    override fun getCount(): Int {
        return mDatas.size
    }

    override fun getItem(position: Int): T {
        return mDatas[position]
    }

    override fun getItemId(position: Int): Long {
        return position.toLong()
    }


}

/**
 * Created by zhy on 16/6/22.
 */
interface ItemViewDelegate<T> {

    val itemViewLayoutId: Int

    fun isForViewType(item: T, position: Int): Boolean

    fun convert(holder: ViewHolder, t: T, position: Int)


}

/**
 * Created by zhy on 16/6/22.
 */
class ItemViewDelegateManager<T> {
    internal var delegates: SparseArrayCompat<ItemViewDelegate<T>> = SparseArrayCompat()

    val itemViewDelegateCount: Int
        get() = delegates.size()

    fun addDelegate(delegate: ItemViewDelegate<T>?): ItemViewDelegateManager<T> {
        var viewType = delegates.size()
        if (delegate != null) {
            delegates.put(viewType, delegate)
            viewType++
        }
        return this
    }

    fun addDelegate(viewType: Int, delegate: ItemViewDelegate<T>): ItemViewDelegateManager<T> {
        if (delegates.get(viewType) != null) {
            throw IllegalArgumentException(
                    "An ItemViewDelegate is already registered for the viewType = "
                            + viewType
                            + ". Already registered ItemViewDelegate is "
                            + delegates.get(viewType))
        }
        delegates.put(viewType, delegate)
        return this
    }

    fun removeDelegate(delegate: ItemViewDelegate<T>?): ItemViewDelegateManager<T> {
        if (delegate == null) {
            throw NullPointerException("ItemViewDelegate is null")
        }
        val indexToRemove = delegates.indexOfValue(delegate)

        if (indexToRemove >= 0) {
            delegates.removeAt(indexToRemove)
        }
        return this
    }

    fun removeDelegate(itemType: Int): ItemViewDelegateManager<T> {
        val indexToRemove = delegates.indexOfKey(itemType)

        if (indexToRemove >= 0) {
            delegates.removeAt(indexToRemove)
        }
        return this
    }

    fun getItemViewType(item: T, position: Int): Int {
        val delegatesCount = delegates.size()
        for (i in delegatesCount - 1 downTo 0) {
            val delegate = delegates.valueAt(i)
            if (delegate.isForViewType(item, position)) {
                return delegates.keyAt(i)
            }
        }
        throw IllegalArgumentException(
                "No ItemViewDelegate added that matches position=$position in data source")
    }

    fun convert(holder: ViewHolder, item: T, position: Int) {
        val delegatesCount = delegates.size()
        for (i in 0 until delegatesCount) {
            val delegate = delegates.valueAt(i)

            if (delegate.isForViewType(item, position)) {
                delegate.convert(holder, item, position)
                return
            }
        }
        throw IllegalArgumentException(
                "No ItemViewDelegateManager added that matches position=$position in data source")
    }


    fun getItemViewLayoutId(viewType: Int): Int {
        val id= delegates[viewType]!!.itemViewLayoutId
        return  id
    }

    fun getItemViewType(itemViewDelegate: ItemViewDelegate<T>): Int {
        return delegates.indexOfValue(itemViewDelegate)
    }

    fun getItemViewDelegate(item: T, position: Int): ItemViewDelegate<T> {
        val delegatesCount = delegates.size()
        for (i in delegatesCount - 1 downTo 0) {
            val delegate = delegates.valueAt(i)
            if (delegate.isForViewType(item, position)) {
                return delegate
            }
        }
        throw IllegalArgumentException(
                "No ItemViewDelegate added that matches position=$position in data source")
    }

    fun getItemViewLayoutId(item: T, position: Int): Int {
        return getItemViewDelegate(item, position).itemViewLayoutId
    }
}
